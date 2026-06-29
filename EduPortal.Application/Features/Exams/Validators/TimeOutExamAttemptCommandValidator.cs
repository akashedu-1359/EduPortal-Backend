using EduPortal.Application.Features.Exams.Commands;
using FluentValidation;

namespace EduPortal.Application.Features.Exams.Validators;

public class TimeOutExamAttemptCommandValidator : AbstractValidator<TimeOutExamAttemptCommand>
{
    public TimeOutExamAttemptCommandValidator()
    {
        RuleFor(x => x.AttemptId).NotEmpty();
    }
}
