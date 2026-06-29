using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using MediatR;

namespace EduPortal.Application.Features.Exams.Queries;

public record GetAdminExamAttemptsQuery(int Page = 1, int PageSize = 20, Guid? ExamId = null)
    : IRequest<Result<PagedResult<AdminAttemptDto>>>;

public record AdminAttemptExamDto(
    Guid Id,
    string Title,
    decimal PassingPercentage,
    int DurationMinutes,
    int MaxAttempts);

public record AdminAttemptDto(
    Guid Id,
    Guid ExamId,
    Guid UserId,
    string Status,
    DateTime StartedAt,
    DateTime? CompletedAt,
    decimal? Score,
    bool? IsPassed,
    AdminAttemptExamDto Exam);

public class GetAdminExamAttemptsQueryHandler : IRequestHandler<GetAdminExamAttemptsQuery, Result<PagedResult<AdminAttemptDto>>>
{
    private readonly IExamRepository _exams;

    public GetAdminExamAttemptsQueryHandler(IExamRepository exams) => _exams = exams;

    public async Task<Result<PagedResult<AdminAttemptDto>>> Handle(
        GetAdminExamAttemptsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, total) = await _exams.GetPagedAttemptsAsync(
            request.Page, request.PageSize, request.ExamId, cancellationToken);

        var dtos = items.Select(a => new AdminAttemptDto(
            a.Id,
            a.ExamId,
            a.UserId,
            a.Status.ToString(),
            a.StartedAt,
            a.CompletedAt,
            a.Score,
            a.IsPassed,
            new AdminAttemptExamDto(
                a.Exam.Id,
                a.Exam.Title,
                a.Exam.PassingPercentage,
                a.Exam.DurationMinutes,
                a.Exam.MaxAttempts))).ToList();

        return Result<PagedResult<AdminAttemptDto>>.Success(
            PagedResult<AdminAttemptDto>.Create(dtos, request.Page, request.PageSize, total));
    }
}
