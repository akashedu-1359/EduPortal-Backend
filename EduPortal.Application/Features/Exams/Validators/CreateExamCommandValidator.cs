using EduPortal.Application.Features.Exams.Commands;
using FluentValidation;

namespace EduPortal.Application.Features.Exams.Validators;

public class CreateExamCommandValidator : AbstractValidator<CreateExamCommand>
{
    public CreateExamCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().Length(3, 255);
        RuleFor(x => x.Description).NotEmpty().MinimumLength(3);
        RuleFor(x => x.DurationMinutes).GreaterThan(0);
        RuleFor(x => x.PassingPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.MaxAttempts).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Questions).NotEmpty().WithMessage("At least one question is required.");
        RuleForEach(x => x.Questions).ChildRules(q =>
        {
            q.RuleFor(x => x.QuestionText).NotEmpty().MinimumLength(3);
            q.RuleFor(x => x.Option1).NotEmpty();
            q.RuleFor(x => x.Option2).NotEmpty();
            q.RuleFor(x => x.Option3).NotEmpty();
            q.RuleFor(x => x.Option4).NotEmpty();
            q.RuleFor(x => x.CorrectOptionIndex).InclusiveBetween(0, 3);
        });
    }
}
