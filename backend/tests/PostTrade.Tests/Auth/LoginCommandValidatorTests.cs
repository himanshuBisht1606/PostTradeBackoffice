using PostTrade.Application.Features.Auth.Commands;

namespace PostTrade.Tests.Auth;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidInput_ShouldPass()
    {
        var command = new LoginCommand("admin", "Admin@123", "DEMO");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenUsernameIsEmpty_ShouldFail()
    {
        var command = new LoginCommand("", "Admin@123", "DEMO");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username");
    }

    [Fact]
    public void Validate_WhenPasswordIsEmpty_ShouldFail()
    {
        var command = new LoginCommand("admin", "", "DEMO");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("12345")]
    public void Validate_WhenPasswordTooShort_ShouldFail(string shortPassword)
    {
        var command = new LoginCommand("admin", shortPassword, "DEMO");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Password" && e.ErrorMessage.Contains("6 characters"));
    }

    [Fact]
    public void Validate_WhenTenantCodeIsEmpty_ShouldFail()
    {
        var command = new LoginCommand("admin", "Admin@123", "");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TenantCode");
    }

    [Fact]
    public void Validate_WhenAllFieldsAreMissing_ShouldReturnMultipleErrors()
    {
        var command = new LoginCommand("", "", "");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(1);
    }
}
