using EduPortal.Application.Features.Auth.Commands;
using FluentValidation;

namespace EduPortal.Application.Features.Auth.Validators;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")
            .WithMessage("Password must contain uppercase, lowercase, and a number.");
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(255);
    }
}
