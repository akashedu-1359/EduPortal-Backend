using EduPortal.Application.Features.Exams.Commands;
using FluentValidation;

namespace EduPortal.Application.Features.Exams.Validators;

public class UpdateQuestionCommandValidator : AbstractValidator<UpdateQuestionCommand>
{
    public UpdateQuestionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.QuestionText).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Option1).NotEmpty();
        RuleFor(x => x.Option2).NotEmpty();
        RuleFor(x => x.Option3).NotEmpty();
        RuleFor(x => x.Option4).NotEmpty();
        RuleFor(x => x.CorrectOptionIndex).InclusiveBetween(0, 3);
    }
}
