using EduPortal.Application.Features.Exams.Commands;
using FluentValidation;

namespace EduPortal.Application.Features.Exams.Validators;

public class UpdateExamCommandValidator : AbstractValidator<UpdateExamCommand>
{
    public UpdateExamCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().Length(3, 255);
        RuleFor(x => x.Description).NotEmpty().MinimumLength(3);
        RuleFor(x => x.DurationMinutes).GreaterThan(0);
        RuleFor(x => x.PassingPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.MaxAttempts).GreaterThanOrEqualTo(0);
    }
}
