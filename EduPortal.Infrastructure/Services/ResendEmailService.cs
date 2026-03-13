using EduPortal.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;

namespace EduPortal.Infrastructure.Services;

public class ResendEmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly string _fromEmail;
    private readonly ILogger<ResendEmailService> _logger;

    public ResendEmailService(IResend resend, IConfiguration config, ILogger<ResendEmailService> logger)
    {
        _resend = resend;
        _fromEmail = config["Email:FromAddress"] ?? "noreply@eduportal.com";
        _logger = logger;
    }

    public async Task SendWelcomeAsync(string toEmail, string fullName, CancellationToken ct = default)
    {
        try
        {
            var message = new EmailMessage
            {
                From = _fromEmail,
                Subject = "Welcome to EduPortal!",
                HtmlBody = $"<h1>Welcome, {fullName}!</h1><p>Your account has been created. Start learning today.</p>"
            };
            message.To.Add(toEmail);
            await _resend.EmailSendAsync(message, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", toEmail);
        }
    }

    public async Task SendCertificateAsync(string toEmail, string fullName, string examTitle, string certificateUrl, CancellationToken ct = default)
    {
        try
        {
            var message = new EmailMessage
            {
                From = _fromEmail,
                Subject = $"Your Certificate for {examTitle}",
                HtmlBody = $"<h1>Congratulations, {fullName}!</h1><p>You passed <strong>{examTitle}</strong>. <a href='{certificateUrl}'>Download your certificate</a>.</p>"
            };
            message.To.Add(toEmail);
            await _resend.EmailSendAsync(message, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send certificate email to {Email}", toEmail);
        }
    }

    public async Task SendPasswordResetAsync(string toEmail, string resetLink, CancellationToken ct = default)
    {
        try
        {
            var message = new EmailMessage
            {
                From = _fromEmail,
                Subject = "Reset your EduPortal password",
                HtmlBody = $"<p>Click <a href='{resetLink}'>here</a> to reset your password. This link expires in 1 hour.</p>"
            };
            message.To.Add(toEmail);
            await _resend.EmailSendAsync(message, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
        }
    }
}
