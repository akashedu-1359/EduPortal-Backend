using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Enums;
using MediatR;

namespace EduPortal.Application.Features.Exams.Commands;

public record TimeOutExamAttemptCommand(Guid AttemptId) : IRequest<Result>;

public class TimeOutExamAttemptCommandHandler : IRequestHandler<TimeOutExamAttemptCommand, Result>
{
    private readonly IExamRepository _exams;
    private readonly ICurrentUserService _currentUser;

    public TimeOutExamAttemptCommandHandler(IExamRepository exams, ICurrentUserService currentUser)
    {
        _exams = exams;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(TimeOutExamAttemptCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;

        var attempt = await _exams.GetAttemptAsync(request.AttemptId, cancellationToken);
        if (attempt == null) return Result.NotFound("Attempt not found.");
        if (attempt.UserId != userId) return Result.Failure("Unauthorized.", 401);

        if (attempt.Status == AttemptStatus.TimedOut)
            return Result.Success();

        if (attempt.Status != AttemptStatus.InProgress)
            return Result.Failure("Attempt is already completed.", 400);

        var scoringInfo = await _exams.GetExamScoringInfoAsync(attempt.ExamId, cancellationToken);
        if (scoringInfo == null) return Result.NotFound("Exam not found.");

        var deadline = attempt.StartedAt.AddMinutes(scoringInfo.DurationMinutes);
        if (DateTime.UtcNow < deadline)
            return Result.Failure("Exam time has not expired yet.", 400);

        attempt.TimeOut();
        await _exams.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
