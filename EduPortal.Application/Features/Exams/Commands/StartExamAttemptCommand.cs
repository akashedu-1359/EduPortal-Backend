using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using MediatR;

namespace EduPortal.Application.Features.Exams.Commands;

public record StartExamAttemptCommand(Guid ExamId) : IRequest<Result<StartAttemptResponse>>;

public record StartAttemptResponse(Guid AttemptId, DateTime StartedAt, DateTime ExpiresAt, List<AttemptQuestionDto> Questions);
public record AttemptQuestionDto(Guid Id, string QuestionText, string Option1, string Option2, string Option3, string Option4, int SortOrder);

public class StartExamAttemptCommandHandler : IRequestHandler<StartExamAttemptCommand, Result<StartAttemptResponse>>
{
    private readonly IExamRepository _exams;
    private readonly ICurrentUserService _currentUser;

    public StartExamAttemptCommandHandler(IExamRepository exams, ICurrentUserService currentUser)
    { _exams = exams; _currentUser = currentUser; }

    public async Task<Result<StartAttemptResponse>> Handle(StartExamAttemptCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;

        var exam = await _exams.GetByIdAsync(request.ExamId, includeQuestions: true, ct: cancellationToken);
        if (exam == null) return Result<StartAttemptResponse>.NotFound("Exam not found.");

        var now = DateTime.UtcNow;
        var unavailable = ExamAvailability.GetUnavailableMessage(exam, now);
        if (unavailable != null)
            return Result<StartAttemptResponse>.Failure(unavailable, 400);

        var activeAttempt = await _exams.GetActiveAttemptAsync(userId, request.ExamId, cancellationToken);
        if (activeAttempt != null)
        {
            var expiresAt = activeAttempt.StartedAt.AddMinutes(activeAttempt.Exam.DurationMinutes);
            if (DateTime.UtcNow >= expiresAt)
            {
                activeAttempt.TimeOut();
                await _exams.SaveChangesAsync(cancellationToken);
            }
            else
            {
                var questions = activeAttempt.Exam.Questions.OrderBy(q => q.SortOrder)
                    .Select(q => new AttemptQuestionDto(q.Id, q.QuestionText, q.Option1, q.Option2, q.Option3, q.Option4, q.SortOrder))
                    .ToList();
                return Result<StartAttemptResponse>.Success(new StartAttemptResponse(activeAttempt.Id, activeAttempt.StartedAt, expiresAt, questions));
            }
        }

        var attemptCount = await _exams.GetAttemptCountAsync(userId, request.ExamId, cancellationToken);
        if (exam.MaxAttempts > 0 && attemptCount >= exam.MaxAttempts)
            return Result<StartAttemptResponse>.Failure($"Maximum attempts ({exam.MaxAttempts}) reached.", 400);

        var attempt = ExamAttempt.Start(userId, request.ExamId);
        await _exams.AddAttemptAsync(attempt, cancellationToken);
        await _exams.SaveChangesAsync(cancellationToken);

        var newQuestions = exam.Questions.OrderBy(q => q.SortOrder)
            .Select(q => new AttemptQuestionDto(q.Id, q.QuestionText, q.Option1, q.Option2, q.Option3, q.Option4, q.SortOrder))
            .ToList();

        var newExpiresAt = attempt.StartedAt.AddMinutes(exam.DurationMinutes);
        return Result<StartAttemptResponse>.Created(new StartAttemptResponse(attempt.Id, attempt.StartedAt, newExpiresAt, newQuestions));
    }
}
