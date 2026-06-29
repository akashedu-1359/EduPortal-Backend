using EduPortal.Application.Common;
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
    private readonly ICmsRepository _cms;
    private readonly IPdfGeneratorService _pdfGenerator;
    private readonly IStorageService _storage;
    private readonly IMediator _mediator;
    private readonly ILogger<ExamPassedDomainEventHandler> _logger;

    public ExamPassedDomainEventHandler(
        IExamRepository exams,
        IUserRepository users,
        ICmsRepository cms,
        IPdfGeneratorService pdfGenerator,
        IStorageService storage,
        IMediator mediator,
        ILogger<ExamPassedDomainEventHandler> logger)
    {
        _exams = exams;
        _users = users;
        _cms = cms;
        _pdfGenerator = pdfGenerator;
        _storage = storage;
        _mediator = mediator;
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
            var pdfBytes = await _pdfGenerator.GenerateCertificateAsync(
                user.FullName, exam.Title, notification.Score, issuedAt, cancellationToken);
            var storageKey = $"certificates/{notification.UserId}/{notification.AttemptId}.pdf";

            using var stream = new MemoryStream(pdfBytes);
            await _storage.UploadAsync(storageKey, stream, "application/pdf", cancellationToken);

            var certificate = Certificate.Create(notification.UserId, notification.AttemptId, storageKey);
            await _exams.AddCertificateAsync(certificate, cancellationToken);
            attempt.CertificateId = certificate.Id;
            await _exams.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Certificate issued for user {UserId}, attempt {AttemptId}.",
                notification.UserId,
                notification.AttemptId);

            var certificatesEnabled = await _cms.IsFeatureEnabledAsync(
                FeatureFlagKeys.EnableCertificates,
                cancellationToken);

            if (certificatesEnabled)
            {
                await _mediator.Send(new SendCertificateEmailCommand(certificate.Id), cancellationToken);
            }
            else
            {
                _logger.LogInformation(
                    "Certificate email deferred for {CertificateId} — {Flag} is disabled.",
                    certificate.Id,
                    FeatureFlagKeys.EnableCertificates);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to issue certificate for attempt {AttemptId}.", notification.AttemptId);
        }
    }
}
