using EduPortal.Application.Features.Exams.Commands;
using FluentValidation;

namespace EduPortal.Application.Features.Exams.Validators;

public class SubmitExamCommandValidator : AbstractValidator<SubmitExamCommand>
{
    public SubmitExamCommandValidator()
    {
        RuleFor(x => x.AttemptId).NotEmpty();
        RuleFor(x => x.Answers).NotEmpty().WithMessage("Answers cannot be empty.");
    }
}
