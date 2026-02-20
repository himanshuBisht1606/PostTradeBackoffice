using FluentValidation.TestHelper;
using PostTrade.Application.Features.Reconciliation.Commands;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.Reconciliation;

public class RunReconciliationCommandValidatorTests
{
    private readonly RunReconciliationCommandValidator _validator = new();

    private static RunReconciliationCommand ValidCommand() => new(
        ReconDate:      DateTime.Today,
        SettlementNo:   "SN001",
        ReconType:      ReconType.Trade,
        SystemValue:    1000m,
        ExchangeValue:  1000m,
        ToleranceLimit: 5m,
        Comments:       null
    );

    [Fact]
    public void Validate_WhenValid_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenSettlementNoEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { SettlementNo = "" });
        result.ShouldHaveValidationErrorFor(x => x.SettlementNo);
    }

    [Fact]
    public void Validate_WhenSystemValueNegative_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { SystemValue = -1m });
        result.ShouldHaveValidationErrorFor(x => x.SystemValue);
    }

    [Fact]
    public void Validate_WhenExchangeValueNegative_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { ExchangeValue = -1m });
        result.ShouldHaveValidationErrorFor(x => x.ExchangeValue);
    }

    [Fact]
    public void Validate_WhenToleranceLimitNegative_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { ToleranceLimit = -1m });
        result.ShouldHaveValidationErrorFor(x => x.ToleranceLimit);
    }
}
