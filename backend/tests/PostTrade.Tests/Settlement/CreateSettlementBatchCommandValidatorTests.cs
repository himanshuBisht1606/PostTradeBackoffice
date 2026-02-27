using FluentValidation.TestHelper;
using PostTrade.Application.Features.Settlement.Batches.Commands;

namespace PostTrade.Tests.Settlement;

public class CreateSettlementBatchCommandValidatorTests
{
    private readonly CreateSettlementBatchCommandValidator _validator = new();

    private static CreateSettlementBatchCommand ValidCommand() => new(
        SettlementNo:   "SN20240101",
        TradeDate:      DateTime.Today,
        SettlementDate: DateTime.Today.AddDays(2),
        ExchangeId:     Guid.NewGuid(),
        TotalTrades:    10,
        TotalTurnover:  500000m
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
    public void Validate_WhenExchangeIdEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { ExchangeId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.ExchangeId);
    }

    [Fact]
    public void Validate_WhenSettlementDateBeforeTradeDate_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { SettlementDate = DateTime.Today.AddDays(-1) });
        result.ShouldHaveValidationErrorFor(x => x.SettlementDate);
    }

    [Fact]
    public void Validate_WhenTotalTradesNegative_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { TotalTrades = -1 });
        result.ShouldHaveValidationErrorFor(x => x.TotalTrades);
    }

    [Fact]
    public void Validate_WhenTotalTurnoverNegative_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { TotalTurnover = -1m });
        result.ShouldHaveValidationErrorFor(x => x.TotalTurnover);
    }
}
