using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using MediatR;

namespace EduPortal.Application.Features.Certificates;

public record GetMyCertificatesQuery : IRequest<Result<List<CertificateDto>>>;
public record CertificateDto(Guid Id, Guid ExamAttemptId, string StorageKey, DateTime IssuedAt);

public class GetMyCertificatesQueryHandler : IRequestHandler<GetMyCertificatesQuery, Result<List<CertificateDto>>>
{
    private readonly IExamRepository _exams;
    private readonly ICurrentUserService _currentUser;

    public GetMyCertificatesQueryHandler(IExamRepository exams, ICurrentUserService currentUser)
    {
        _exams = exams;
        _currentUser = currentUser;
    }

    public async Task<Result<List<CertificateDto>>> Handle(GetMyCertificatesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;
        var certs = await _exams.GetCertificatesByUserIdAsync(userId, cancellationToken);
        var dtos = certs.Select(c => new CertificateDto(c.Id, c.ExamAttemptId, c.StorageKey, c.IssuedAt)).ToList();
        return Result<List<CertificateDto>>.Success(dtos);
    }
}
