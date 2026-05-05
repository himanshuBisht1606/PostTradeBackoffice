using FluentValidation.TestHelper;
using PostTrade.Application.Features.MasterSetup.Brokers.Commands;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.MasterSetup.Brokers;

public class CreateBrokerCommandValidatorTests
{
    private readonly CreateBrokerCommandValidator _validator = new();

    private static CreateBrokerCommand ValidCommand() => new(
        BrokerCode:                     "BRK001",
        BrokerName:                     "Alpha Brokers",
        EntityType:                     BrokerEntityType.PrivateLimited,
        Website:                        null,
        ContactEmail:                   "contact@alpha.com",
        ContactPhone:                   "9876543210",
        CIN:                            null,
        TAN:                            null,
        PAN:                            null,
        GST:                            null,
        IncorporationDate:              null,
        RegisteredAddressLine1:         null,
        RegisteredAddressLine2:         null,
        RegisteredCity:                 null,
        RegisteredState:                null,
        RegisteredPinCode:              null,
        RegisteredCountry:              "India",
        CorrespondenceSameAsRegistered: true,
        CorrespondenceAddressLine1:     null,
        CorrespondenceAddressLine2:     null,
        CorrespondenceCity:             null,
        CorrespondenceState:            null,
        CorrespondencePinCode:          null,
        SEBIRegistrationNo:             null,
        SEBIRegistrationDate:           null,
        SEBIRegistrationExpiry:         null,
        ComplianceOfficerName:          null,
        ComplianceOfficerEmail:         null,
        ComplianceOfficerPhone:         null,
        PrincipalOfficerName:           null,
        PrincipalOfficerEmail:          null,
        PrincipalOfficerPhone:          null,
        SettlementBankName:             null,
        SettlementBankAccountNo:        null,
        SettlementBankIfsc:             null,
        SettlementBankBranch:           null
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

    [Fact]
    public void Validate_WhenPanInvalidFormat_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { PAN = "INVALID" });
        result.ShouldHaveValidationErrorFor(x => x.PAN);
    }

    [Fact]
    public void Validate_WhenPanValidFormat_ShouldPass()
    {
        var result = _validator.TestValidate(ValidCommand() with { PAN = "AAAAA9999A" });
        result.ShouldNotHaveValidationErrorFor(x => x.PAN);
    }
}
