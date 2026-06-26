using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using MediatR;

namespace EduPortal.Application.Features.Exams.Commands;

public record UpdateQuestionCommand(
    Guid Id, string QuestionText,
    string Option1, string Option2, string Option3, string Option4,
    int CorrectOptionIndex, int SortOrder) : IRequest<Result>;

public class UpdateQuestionCommandHandler : IRequestHandler<UpdateQuestionCommand, Result>
{
    private readonly IExamRepository _exams;

    public UpdateQuestionCommandHandler(IExamRepository exams) => _exams = exams;

    public async Task<Result> Handle(UpdateQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _exams.GetQuestionByIdAsync(request.Id, cancellationToken);
        if (question == null) return Result.NotFound("Question not found.");

        question.QuestionText = request.QuestionText;
        question.Option1 = request.Option1;
        question.Option2 = request.Option2;
        question.Option3 = request.Option3;
        question.Option4 = request.Option4;
        question.UpdateCorrectOption(request.CorrectOptionIndex);
        question.SortOrder = request.SortOrder;

        await _exams.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
