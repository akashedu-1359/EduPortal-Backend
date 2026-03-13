using EduPortal.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EduPortal.Infrastructure.Services;

public class QuestPdfCertificateService : IPdfGeneratorService
{
    public Task<byte[]> GenerateCertificateAsync(string userName, string examTitle, decimal score, DateTime issuedAt, CancellationToken ct = default)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(50);
                page.Background().Background(Colors.White);

                page.Content().Column(col =>
                {
                    col.Item().AlignCenter().Text("Certificate of Completion")
                        .FontSize(36).Bold().FontColor(Colors.Blue.Darken3);

                    col.Item().Height(20);
                    col.Item().AlignCenter().Text("This certifies that").FontSize(16);

                    col.Item().Height(10);
                    col.Item().AlignCenter().Text(userName).FontSize(28).Bold();

                    col.Item().Height(10);
                    col.Item().AlignCenter().Text($"has successfully completed").FontSize(16);

                    col.Item().Height(10);
                    col.Item().AlignCenter().Text(examTitle).FontSize(22).Bold().FontColor(Colors.Blue.Medium);

                    col.Item().Height(10);
                    col.Item().AlignCenter().Text($"with a score of {score:F1}%").FontSize(16);

                    col.Item().Height(30);
                    col.Item().AlignCenter().Text($"Issued on {issuedAt:MMMM dd, yyyy}").FontSize(12).FontColor(Colors.Grey.Darken1);
                });
            });
        });

        var bytes = pdf.GeneratePdf();
        return Task.FromResult(bytes);
    }
}
