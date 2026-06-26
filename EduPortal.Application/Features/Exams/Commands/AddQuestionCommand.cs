using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using MediatR;

namespace EduPortal.Application.Features.Exams.Commands;

public record AddQuestionCommand(
    Guid ExamId, string QuestionText,
    string Option1, string Option2, string Option3, string Option4,
    int CorrectOptionIndex, int SortOrder) : IRequest<Result<Guid>>;

public class AddQuestionCommandHandler : IRequestHandler<AddQuestionCommand, Result<Guid>>
{
    private readonly IExamRepository _exams;

    public AddQuestionCommandHandler(IExamRepository exams) => _exams = exams;

    public async Task<Result<Guid>> Handle(AddQuestionCommand request, CancellationToken cancellationToken)
    {
        var exam = await _exams.GetByIdAsync(request.ExamId, includeQuestions: true, ct: cancellationToken);
        if (exam == null) return Result<Guid>.NotFound("Exam not found.");
        if (exam.Status == ExamStatus.Active) return Result<Guid>.Failure("Cannot add questions to an active exam.", 400);

        var question = new Question(
            request.ExamId, request.QuestionText,
            request.Option1, request.Option2, request.Option3, request.Option4,
            request.CorrectOptionIndex, request.SortOrder);

        exam.Questions.Add(question);
        await _exams.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Created(question.Id);
    }
}
