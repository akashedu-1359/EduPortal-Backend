using EduPortal.Application.Features.Exams.Commands;
using FluentValidation;

namespace EduPortal.Application.Features.Exams.Validators;

public class BulkAddQuestionsCommandValidator : AbstractValidator<BulkAddQuestionsCommand>
{
    public BulkAddQuestionsCommandValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.Questions).NotEmpty();
        RuleForEach(x => x.Questions).ChildRules(q =>
        {
            q.RuleFor(x => x.QuestionText).NotEmpty().MinimumLength(3);
            q.RuleFor(x => x.Option1).NotEmpty();
            q.RuleFor(x => x.Option2).NotEmpty();
            q.RuleFor(x => x.Option3).NotEmpty();
            q.RuleFor(x => x.Option4).NotEmpty();
            q.RuleFor(x => x.CorrectOptionIndex).InclusiveBetween(0, 3);
            q.RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
        });
    }
}
