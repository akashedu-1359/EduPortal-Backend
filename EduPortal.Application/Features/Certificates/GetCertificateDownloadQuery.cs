using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using MediatR;

namespace EduPortal.Application.Features.Certificates;

public record GetCertificateDownloadQuery(Guid CertificateId) : IRequest<Result<string>>;

public class GetCertificateDownloadQueryHandler : IRequestHandler<GetCertificateDownloadQuery, Result<string>>
{
    private readonly IExamRepository _exams;
    private readonly IStorageService _storage;
    private readonly ICurrentUserService _currentUser;

    public GetCertificateDownloadQueryHandler(IExamRepository exams, IStorageService storage, ICurrentUserService currentUser)
    {
        _exams = exams;
        _storage = storage;
        _currentUser = currentUser;
    }

    public async Task<Result<string>> Handle(GetCertificateDownloadQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;
        var cert = await _exams.GetCertificateAsync(request.CertificateId, cancellationToken);
        if (cert == null) return Result<string>.NotFound("Certificate not found.");
        if (cert.UserId != userId) return Result<string>.Unauthorized("Access denied.");

        var url = await _storage.GetReadUrlAsync(cert.StorageKey, 300, cancellationToken);
        return Result<string>.Success(url);
    }
}
