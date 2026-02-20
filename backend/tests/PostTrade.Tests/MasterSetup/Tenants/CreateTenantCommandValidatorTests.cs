using FluentValidation.TestHelper;
using PostTrade.Application.Features.MasterSetup.Tenants.Commands;

namespace PostTrade.Tests.MasterSetup.Tenants;

public class CreateTenantCommandValidatorTests
{
    private readonly CreateTenantCommandValidator _validator = new();

    private static CreateTenantCommand ValidCommand() => new(
        TenantCode:   "TEN001",
        TenantName:   "Demo Corp",
        ContactEmail: "admin@democorp.com",
        ContactPhone: "9876543210",
        Address:      null,
        City:         null,
        Country:      null,
        TaxId:        null
    );

    [Fact]
    public void Validate_WhenValid_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenTenantCodeEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { TenantCode = "" });
        result.ShouldHaveValidationErrorFor(x => x.TenantCode);
    }

    [Fact]
    public void Validate_WhenTenantNameEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { TenantName = "" });
        result.ShouldHaveValidationErrorFor(x => x.TenantName);
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
        var result = _validator.TestValidate(ValidCommand() with { ContactEmail = "not-an-email" });
        result.ShouldHaveValidationErrorFor(x => x.ContactEmail);
    }

    [Fact]
    public void Validate_WhenContactPhoneEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { ContactPhone = "" });
        result.ShouldHaveValidationErrorFor(x => x.ContactPhone);
    }
}
