using EduPortal.Application.Features.Auth.Commands;
using EduPortal.Application.Features.Auth.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace EduPortal.Tests.Validators;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var command = new RegisterCommand("user@example.com", "Password1", "John Doe");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyEmail_HasError()
    {
        var command = new RegisterCommand("", "Password1", "John Doe");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_InvalidEmailFormat_HasError()
    {
        var command = new RegisterCommand("not-an-email", "Password1", "John Doe");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_EmailTooLong_HasError()
    {
        var longEmail = new string('a', 247) + "@test.com";
        var command = new RegisterCommand(longEmail, "Password1", "John Doe");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_EmptyPassword_HasError()
    {
        var command = new RegisterCommand("user@example.com", "", "John Doe");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_PasswordTooShort_HasError()
    {
        var command = new RegisterCommand("user@example.com", "Pass1", "John Doe");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_PasswordNoUppercase_HasError()
    {
        var command = new RegisterCommand("user@example.com", "password1", "John Doe");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_PasswordNoLowercase_HasError()
    {
        var command = new RegisterCommand("user@example.com", "PASSWORD1", "John Doe");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_PasswordNoDigit_HasError()
    {
        var command = new RegisterCommand("user@example.com", "Passworddd", "John Doe");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_EmptyFullName_HasError()
    {
        var command = new RegisterCommand("user@example.com", "Password1", "");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Validate_FullNameTooLong_HasError()
    {
        var longName = new string('A', 256);
        var command = new RegisterCommand("user@example.com", "Password1", longName);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user@domain.co.uk")]
    [InlineData("first+last@company.org")]
    public void Validate_ValidEmails_HasNoEmailError(string email)
    {
        var command = new RegisterCommand(email, "Password1", "John Doe");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("Password1")]
    [InlineData("StrongP4ss")]
    [InlineData("MyP@ssw0rd")]
    public void Validate_ValidPasswords_HasNoPasswordError(string password)
    {
        var command = new RegisterCommand("user@example.com", password, "John Doe");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}
