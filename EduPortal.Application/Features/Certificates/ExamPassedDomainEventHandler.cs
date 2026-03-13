using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EduPortal.Application.Features.Certificates;

public class ExamPassedDomainEventHandler : INotificationHandler<ExamPassedDomainEvent>
{
    private readonly IExamRepository _exams;
    private readonly IUserRepository _users;
    private readonly IPdfGeneratorService _pdfGenerator;
    private readonly IStorageService _storage;
    private readonly IEmailService _email;
    private readonly ILogger<ExamPassedDomainEventHandler> _logger;

    public ExamPassedDomainEventHandler(
        IExamRepository exams,
        IUserRepository users,
        IPdfGeneratorService pdfGenerator,
        IStorageService storage,
        IEmailService email,
        ILogger<ExamPassedDomainEventHandler> logger)
    {
        _exams = exams;
        _users = users;
        _pdfGenerator = pdfGenerator;
        _storage = storage;
        _email = email;
        _logger = logger;
    }

    public async Task Handle(ExamPassedDomainEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var attempt = await _exams.GetAttemptAsync(notification.AttemptId, cancellationToken);
            if (attempt == null) return;

            var exam = await _exams.GetByIdAsync(notification.ExamId, false, cancellationToken);
            var user = await _users.GetByIdAsync(notification.UserId, cancellationToken);
            if (exam == null || user == null) return;

            var issuedAt = attempt.CompletedAt ?? DateTime.UtcNow;
            var pdfBytes = await _pdfGenerator.GenerateCertificateAsync(user.FullName, exam.Title, notification.Score, issuedAt, cancellationToken);
            var storageKey = $"certificates/{notification.UserId}/{notification.AttemptId}.pdf";

            using var stream = new MemoryStream(pdfBytes);
            await _storage.UploadAsync(storageKey, stream, "application/pdf", cancellationToken);

            var certificate = Certificate.Create(notification.UserId, notification.AttemptId, storageKey);
            await _exams.AddCertificateAsync(certificate, cancellationToken);
            attempt.CertificateId = certificate.Id;
            await _exams.SaveChangesAsync(cancellationToken);

            var certUrl = await _storage.GetReadUrlAsync(storageKey, 3600, cancellationToken);
            await _email.SendCertificateEmailAsync(user.Email, user.FullName, certUrl, cancellationToken);

            _logger.LogInformation("Certificate issued for user {UserId}, attempt {AttemptId}.", notification.UserId, notification.AttemptId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to issue certificate for attempt {AttemptId}.", notification.AttemptId);
        }
    }
}
