using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using MediatR;

namespace EduPortal.Application.Features.Certificates;

public record GetAdminCertificatesQuery(int Page = 1, int PageSize = 20, string? Search = null)
    : IRequest<Result<PagedResult<AdminCertificateDto>>>;

public record AdminCertificateDto(
    Guid Id,
    Guid UserId,
    string UserName,
    string UserEmail,
    Guid ExamId,
    string ExamTitle,
    Guid ExamAttemptId,
    DateTime IssuedAt);

public class GetAdminCertificatesQueryHandler : IRequestHandler<GetAdminCertificatesQuery, Result<PagedResult<AdminCertificateDto>>>
{
    private readonly IExamRepository _exams;

    public GetAdminCertificatesQueryHandler(IExamRepository exams) => _exams = exams;

    public async Task<Result<PagedResult<AdminCertificateDto>>> Handle(
        GetAdminCertificatesQuery request,
        CancellationToken cancellationToken)
    {
        var (items, total) = await _exams.GetPagedCertificatesAsync(
            request.Page,
            request.PageSize,
            request.Search,
            cancellationToken);

        var dtos = items.Select(c => new AdminCertificateDto(
            c.Id,
            c.UserId,
            c.User?.FullName ?? "",
            c.User?.Email ?? "",
            c.ExamAttempt?.ExamId ?? Guid.Empty,
            c.ExamAttempt?.Exam?.Title ?? "",
            c.ExamAttemptId,
            c.IssuedAt)).ToList();

        return Result<PagedResult<AdminCertificateDto>>.Success(
            PagedResult<AdminCertificateDto>.Create(dtos, request.Page, request.PageSize, total));
    }
}
