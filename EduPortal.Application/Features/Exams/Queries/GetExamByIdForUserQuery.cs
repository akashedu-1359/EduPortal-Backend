using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Enums;
using MediatR;

namespace EduPortal.Application.Features.Exams.Queries;

public record GetExamByIdForUserQuery(Guid ExamId) : IRequest<Result<UserExamDetailDto>>;

public record UserExamDetailDto(
    Guid Id, string Title, string Description, int DurationMinutes,
    decimal PassingPercentage, int MaxAttempts, string Status,
    int QuestionCount, int UserAttemptCount, DateTime CreatedAt);

public class GetExamByIdForUserQueryHandler : IRequestHandler<GetExamByIdForUserQuery, Result<UserExamDetailDto>>
{
    private readonly IExamRepository _exams;
    private readonly ICurrentUserService _currentUser;

    public GetExamByIdForUserQueryHandler(IExamRepository exams, ICurrentUserService currentUser)
    { _exams = exams; _currentUser = currentUser; }

    public async Task<Result<UserExamDetailDto>> Handle(GetExamByIdForUserQuery request, CancellationToken cancellationToken)
    {
        var exam = await _exams.GetByIdAsync(request.ExamId, includeQuestions: true, ct: cancellationToken);
        if (exam == null || exam.Status != ExamStatus.Active)
            return Result<UserExamDetailDto>.NotFound("Exam not found.");

        var userId = _currentUser.UserId ?? Guid.Empty;
        var attemptCount = await _exams.GetAttemptCountAsync(userId, request.ExamId, cancellationToken);

        var dto = new UserExamDetailDto(
            exam.Id, exam.Title, exam.Description, exam.DurationMinutes,
            exam.PassingPercentage, exam.MaxAttempts, exam.Status.ToString(),
            exam.Questions.Count, attemptCount, exam.CreatedAt);

        return Result<UserExamDetailDto>.Success(dto);
    }
}
