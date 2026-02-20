using FluentValidation.TestHelper;
using PostTrade.Application.Features.MasterSetup.Brokers.Commands;

namespace PostTrade.Tests.MasterSetup.Brokers;

public class CreateBrokerCommandValidatorTests
{
    private readonly CreateBrokerCommandValidator _validator = new();

    private static CreateBrokerCommand ValidCommand() => new(
        BrokerCode:         "BRK001",
        BrokerName:         "Alpha Brokers",
        ContactEmail:       "contact@alpha.com",
        ContactPhone:       "9876543210",
        SEBIRegistrationNo: null,
        Address:            null,
        PAN:                null,
        GST:                null
    );

    [Fact]
    public void Validate_WhenValid_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenBrokerCodeEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { BrokerCode = "" });
        result.ShouldHaveValidationErrorFor(x => x.BrokerCode);
    }

    [Fact]
    public void Validate_WhenBrokerNameEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { BrokerName = "" });
        result.ShouldHaveValidationErrorFor(x => x.BrokerName);
    }

    [Fact]
    public void Validate_WhenContactEmailEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { ContactEmail = "" });
        result.ShouldHaveValidationErrorFor(x => x.ContactEmail);
    }

    [Fact]
    public void Validate_WhenContactEmailInvalid_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { ContactEmail = "invalid-email" });
        result.ShouldHaveValidationErrorFor(x => x.ContactEmail);
    }

    [Fact]
    public void Validate_WhenContactPhoneEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { ContactPhone = "" });
        result.ShouldHaveValidationErrorFor(x => x.ContactPhone);
    }
}
