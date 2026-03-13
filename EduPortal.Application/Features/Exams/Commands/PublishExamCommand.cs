using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Enums;
using MediatR;

namespace EduPortal.Application.Features.Exams.Commands;

public record PublishExamCommand(Guid Id) : IRequest<Result>;
public record UnpublishExamCommand(Guid Id) : IRequest<Result>;

public class PublishExamCommandHandler : IRequestHandler<PublishExamCommand, Result>
{
    private readonly IExamRepository _exams;
    public PublishExamCommandHandler(IExamRepository exams) => _exams = exams;

    public async Task<Result> Handle(PublishExamCommand request, CancellationToken cancellationToken)
    {
        var exam = await _exams.GetByIdAsync(request.Id, includeQuestions: true, ct: cancellationToken);
        if (exam == null) return Result.NotFound("Exam not found.");
        if (exam.Status == ExamStatus.Active) return Result.Failure("Exam is already active.", 409);
        if (!exam.Questions.Any()) return Result.Failure("Exam must have at least one question before activating.", 400);
        exam.Status = ExamStatus.Active;
        await _exams.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class UnpublishExamCommandHandler : IRequestHandler<UnpublishExamCommand, Result>
{
    private readonly IExamRepository _exams;
    public UnpublishExamCommandHandler(IExamRepository exams) => _exams = exams;

    public async Task<Result> Handle(UnpublishExamCommand request, CancellationToken cancellationToken)
    {
        var exam = await _exams.GetByIdAsync(request.Id, ct: cancellationToken);
        if (exam == null) return Result.NotFound("Exam not found.");
        if (exam.Status != ExamStatus.Active) return Result.Failure("Exam is not currently active.", 400);
        exam.Status = ExamStatus.Draft;
        await _exams.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
