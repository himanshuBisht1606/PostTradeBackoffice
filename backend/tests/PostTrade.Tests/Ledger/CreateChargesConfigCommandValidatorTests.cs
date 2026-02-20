using FluentValidation.TestHelper;
using PostTrade.Application.Features.Ledger.Charges.Commands;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.Ledger;

public class CreateChargesConfigCommandValidatorTests
{
    private readonly CreateChargesConfigCommandValidator _validator = new();

    private static CreateChargesConfigCommand ValidCommand() => new(
        BrokerId:        Guid.NewGuid(),
        ChargeName:      "Brokerage",
        ChargeType:      ChargeType.Brokerage,
        CalculationType: CalculationType.Percentage,
        Rate:            0.5m,
        MinAmount:       50m,
        MaxAmount:       5000m,
        EffectiveFrom:   DateTime.Today,
        EffectiveTo:     DateTime.Today.AddYears(1)
    );

    [Fact]
    public void Validate_WhenValid_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenChargeNameEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { ChargeName = "" });
        result.ShouldHaveValidationErrorFor(x => x.ChargeName);
    }

    [Fact]
    public void Validate_WhenRateIsNegative_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { Rate = -0.1m });
        result.ShouldHaveValidationErrorFor(x => x.Rate);
    }

    [Fact]
    public void Validate_WhenMaxAmountLessThanMinAmount_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { MinAmount = 5000m, MaxAmount = 50m });
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validate_WhenEffectiveToBeforeEffectiveFrom_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { EffectiveTo = DateTime.Today.AddDays(-1) });
        result.ShouldHaveValidationErrorFor(x => x.EffectiveTo);
    }
}
