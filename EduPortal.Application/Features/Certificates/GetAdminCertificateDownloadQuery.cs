using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using MediatR;

namespace EduPortal.Application.Features.Certificates;

public record GetAdminCertificateDownloadQuery(Guid CertificateId) : IRequest<Result<string>>;

public class GetAdminCertificateDownloadQueryHandler : IRequestHandler<GetAdminCertificateDownloadQuery, Result<string>>
{
    private readonly IExamRepository _exams;
    private readonly IStorageService _storage;

    public GetAdminCertificateDownloadQueryHandler(IExamRepository exams, IStorageService storage)
    {
        _exams = exams;
        _storage = storage;
    }

    public async Task<Result<string>> Handle(GetAdminCertificateDownloadQuery request, CancellationToken cancellationToken)
    {
        var cert = await _exams.GetCertificateAsync(request.CertificateId, cancellationToken);
        if (cert == null) return Result<string>.NotFound("Certificate not found.");

        var url = await _storage.GetReadUrlAsync(cert.StorageKey, 300, cancellationToken);
        return Result<string>.Success(url);
    }
}
