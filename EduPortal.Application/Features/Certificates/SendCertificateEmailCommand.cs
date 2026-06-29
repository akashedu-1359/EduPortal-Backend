using EduPortal.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EduPortal.Application.Features.Certificates;

public record SendCertificateEmailCommand(Guid CertificateId) : IRequest<bool>;

public class SendCertificateEmailCommandHandler : IRequestHandler<SendCertificateEmailCommand, bool>
{
    private readonly IExamRepository _exams;
    private readonly IUserRepository _users;
    private readonly IStorageService _storage;
    private readonly IEmailService _email;
    private readonly ILogger<SendCertificateEmailCommandHandler> _logger;

    public SendCertificateEmailCommandHandler(
        IExamRepository exams,
        IUserRepository users,
        IStorageService storage,
        IEmailService email,
        ILogger<SendCertificateEmailCommandHandler> logger)
    {
        _exams = exams;
        _users = users;
        _storage = storage;
        _email = email;
        _logger = logger;
    }

    public async Task<bool> Handle(SendCertificateEmailCommand request, CancellationToken cancellationToken)
    {
        var certificate = await _exams.GetCertificateAsync(request.CertificateId, cancellationToken);
        if (certificate == null || certificate.EmailSentAt != null)
            return false;

        var user = await _users.GetByIdAsync(certificate.UserId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("Certificate {CertificateId} has no user; skipping email.", request.CertificateId);
            return false;
        }

        try
        {
            var certUrl = await _storage.GetReadUrlAsync(certificate.StorageKey, 3600, cancellationToken);
            await _email.SendCertificateEmailAsync(user.Email, user.FullName, certUrl, cancellationToken);
            certificate.MarkEmailSent();
            await _exams.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send certificate email for {CertificateId}.", request.CertificateId);
            return false;
        }
    }
}
