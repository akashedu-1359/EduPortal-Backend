using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using MediatR;

namespace EduPortal.Application.Features.Exams.Commands;

public record AnswerSubmission(Guid QuestionId, int? SelectedOptionIndex);
public record SubmitExamCommand(Guid AttemptId, List<AnswerSubmission> Answers) : IRequest<Result<SubmitExamResponse>>;
public record SubmitExamResponse(decimal Score, bool IsPassed, decimal PassingPercentage, int TotalQuestions, int CorrectAnswers);

public class SubmitExamCommandHandler : IRequestHandler<SubmitExamCommand, Result<SubmitExamResponse>>
{
    private readonly IExamRepository _exams;
    private readonly ICurrentUserService _currentUser;
    private readonly IPublisher _publisher;

    public SubmitExamCommandHandler(IExamRepository exams, ICurrentUserService currentUser, IPublisher publisher)
    { _exams = exams; _currentUser = currentUser; _publisher = publisher; }

    public async Task<Result<SubmitExamResponse>> Handle(SubmitExamCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;

        var attempt = await _exams.GetAttemptAsync(request.AttemptId, cancellationToken);
        if (attempt == null) return Result<SubmitExamResponse>.NotFound("Attempt not found.");
        if (attempt.UserId != userId) return Result<SubmitExamResponse>.Unauthorized();
        if (attempt.Status != AttemptStatus.InProgress) return Result<SubmitExamResponse>.Failure("Attempt is already completed.", 400);

        var scoringInfo = await _exams.GetExamScoringInfoAsync(attempt.ExamId, cancellationToken);
        if (scoringInfo == null) return Result<SubmitExamResponse>.NotFound("Exam not found.");

        var deadline = attempt.StartedAt.AddMinutes(scoringInfo.DurationMinutes).AddSeconds(30);
        if (DateTime.UtcNow > deadline)
        {
            attempt.TimeOut();
            await _exams.SaveChangesAsync(cancellationToken);
            return Result<SubmitExamResponse>.Failure("Exam time has expired.", 400);
        }

        var questionMap = scoringInfo.Questions.ToDictionary(q => q.Id);
        int correct = 0;

        foreach (var submission in request.Answers)
        {
            if (!questionMap.TryGetValue(submission.QuestionId, out var question)) continue;
            var answer = AttemptAnswer.Create(attempt.Id, question.Id, submission.SelectedOptionIndex, question.CorrectOptionIndex);
            await _exams.AddAttemptAnswerAsync(answer, cancellationToken);
            if (answer.IsCorrect) correct++;
        }

        var totalQuestions = scoringInfo.Questions.Count;
        var score = totalQuestions > 0
            ? Math.Round((decimal)correct / totalQuestions * 100, 2)
            : 0m;

        attempt.Complete(score, scoringInfo.PassingPercentage);

        await _exams.SaveChangesAsync(cancellationToken);

        foreach (var evt in attempt.DomainEvents)
            if (evt is INotification notification)
                await _publisher.Publish(notification, cancellationToken);
        attempt.ClearDomainEvents();

        return Result<SubmitExamResponse>.Success(new SubmitExamResponse(
            score,
            attempt.IsPassed ?? false,
            scoringInfo.PassingPercentage,
            totalQuestions,
            correct));
    }
}
