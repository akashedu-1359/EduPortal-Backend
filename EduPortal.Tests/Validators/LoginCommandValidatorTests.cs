using EduPortal.Application.Features.Auth.Commands;
using EduPortal.Application.Features.Auth.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace EduPortal.Tests.Validators;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var command = new LoginCommand("user@example.com", "password123");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyEmail_HasError()
    {
        var command = new LoginCommand("", "password123");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_InvalidEmailFormat_HasError()
    {
        var command = new LoginCommand("not-an-email", "password123");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_EmptyPassword_HasError()
    {
        var command = new LoginCommand("user@example.com", "");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("admin@domain.org")]
    [InlineData("test.user+tag@company.co.uk")]
    public void Validate_ValidEmails_HasNoEmailError(string email)
    {
        var command = new LoginCommand(email, "password123");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_EmailWithoutAtSign_HasError()
    {
        var command = new LoginCommand("userdomain.com", "password123");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_AnyNonEmptyPassword_IsValid()
    {
        var command = new LoginCommand("user@example.com", "a");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_BothFieldsEmpty_HasMultipleErrors()
    {
        var command = new LoginCommand("", "");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
