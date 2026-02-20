using FluentValidation.TestHelper;
using PostTrade.Application.Features.MasterSetup.Users.Commands;

namespace PostTrade.Tests.MasterSetup.Users;

public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator = new();

    private static CreateUserCommand ValidCommand() => new(
        Username:  "jdoe",
        Email:     "jdoe@example.com",
        Password:  "secret123",
        FirstName: "John",
        LastName:  "Doe",
        Phone:     null
    );

    [Fact]
    public void Validate_WhenValid_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenUsernameEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { Username = "" });
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Validate_WhenEmailEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { Email = "" });
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WhenEmailInvalid_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { Email = "not-an-email" });
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WhenPasswordEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { Password = "" });
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("12345")]
    public void Validate_WhenPasswordTooShort_ShouldFail(string shortPassword)
    {
        var result = _validator.TestValidate(ValidCommand() with { Password = shortPassword });
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WhenFirstNameEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { FirstName = "" });
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Validate_WhenLastNameEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { LastName = "" });
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }
}
