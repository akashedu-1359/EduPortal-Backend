namespace EduPortal.Application.Interfaces;

public interface IEmailService
{
    Task SendWelcomeAsync(string toEmail, string fullName, CancellationToken ct = default);
    Task SendCertificateAsync(string toEmail, string fullName, string examTitle, string certificateUrl, CancellationToken ct = default);
    Task SendCertificateEmailAsync(string toEmail, string fullName, string certificateUrl, CancellationToken ct = default);
    Task SendPasswordResetAsync(string toEmail, string resetLink, CancellationToken ct = default);
}
