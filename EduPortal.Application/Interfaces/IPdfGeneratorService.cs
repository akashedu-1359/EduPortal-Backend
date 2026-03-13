namespace EduPortal.Application.Interfaces;

public interface IPdfGeneratorService
{
    Task<byte[]> GenerateCertificateAsync(string userName, string examTitle, decimal score, DateTime issuedAt, CancellationToken ct = default);
}
