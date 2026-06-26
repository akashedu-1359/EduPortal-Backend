using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Enums;
using MediatR;

namespace EduPortal.Application.Features.Exams.Commands;

public record DeleteQuestionCommand(Guid Id) : IRequest<Result>;

public class DeleteQuestionCommandHandler : IRequestHandler<DeleteQuestionCommand, Result>
{
    private readonly IExamRepository _exams;

    public DeleteQuestionCommandHandler(IExamRepository exams) => _exams = exams;

    public async Task<Result> Handle(DeleteQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _exams.GetQuestionByIdAsync(request.Id, cancellationToken);
        if (question == null) return Result.NotFound("Question not found.");

        var exam = await _exams.GetByIdAsync(question.ExamId, ct: cancellationToken);
        if (exam != null && exam.Status == ExamStatus.Active)
            return Result.Failure("Cannot delete questions from an active exam.", 400);

        _exams.RemoveQuestion(question);
        await _exams.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
