using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Enums;
using MediatR;

namespace EduPortal.Application.Features.Exams.Commands;

public record UpdateExamCommand(
    Guid Id,
    string Title,
    string Description,
    int DurationMinutes,
    decimal PassingPercentage,
    int MaxAttempts,
    DateTime? ScheduledStartAt,
    DateTime? ScheduledEndAt) : IRequest<Result>;

public class UpdateExamCommandHandler : IRequestHandler<UpdateExamCommand, Result>
{
    private readonly IExamRepository _exams;

    public UpdateExamCommandHandler(IExamRepository exams) => _exams = exams;

    public async Task<Result> Handle(UpdateExamCommand request, CancellationToken cancellationToken)
    {
        var exam = await _exams.GetByIdAsync(request.Id, ct: cancellationToken);
        if (exam == null) return Result.NotFound("Exam not found.");
        if (exam.Status == ExamStatus.Active) return Result.Failure("Cannot edit an active exam.", 400);

        exam.Title = request.Title;
        exam.Description = request.Description;
        exam.DurationMinutes = request.DurationMinutes;
        exam.PassingPercentage = request.PassingPercentage;
        exam.MaxAttempts = request.MaxAttempts;
        exam.ScheduledStartAt = request.ScheduledStartAt;
        exam.ScheduledEndAt = request.ScheduledEndAt;

        await _exams.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
