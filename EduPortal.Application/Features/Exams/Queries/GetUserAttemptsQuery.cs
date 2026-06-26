using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using MediatR;

namespace EduPortal.Application.Features.Exams.Queries;

public record GetUserAttemptsQuery(int Page = 1, int PageSize = 20) : IRequest<Result<PagedResult<UserAttemptDto>>>;

public record UserAttemptDto(
    Guid Id, Guid ExamId, string ExamTitle, DateTime StartedAt,
    DateTime? CompletedAt, string Status, decimal? Score, bool? IsPassed);

public class GetUserAttemptsQueryHandler : IRequestHandler<GetUserAttemptsQuery, Result<PagedResult<UserAttemptDto>>>
{
    private readonly IExamRepository _exams;
    private readonly ICurrentUserService _currentUser;

    public GetUserAttemptsQueryHandler(IExamRepository exams, ICurrentUserService currentUser)
    { _exams = exams; _currentUser = currentUser; }

    public async Task<Result<PagedResult<UserAttemptDto>>> Handle(GetUserAttemptsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;
        var (items, total) = await _exams.GetPagedAttemptsByUserIdAsync(userId, request.Page, request.PageSize, cancellationToken);

        var dtos = items.Select(a => new UserAttemptDto(
            a.Id, a.ExamId, a.Exam.Title, a.StartedAt,
            a.CompletedAt, a.Status.ToString(), a.Score, a.IsPassed)).ToList();

        return Result<PagedResult<UserAttemptDto>>.Success(
            PagedResult<UserAttemptDto>.Create(dtos, request.Page, request.PageSize, total));
    }
}
