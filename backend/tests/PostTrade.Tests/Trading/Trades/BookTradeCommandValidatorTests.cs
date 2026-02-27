using FluentValidation.TestHelper;
using PostTrade.Application.Features.Trading.Trades.Commands;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.Trading.Trades;

public class BookTradeCommandValidatorTests
{
    private readonly BookTradeCommandValidator _validator = new();

    private static BookTradeCommand ValidCommand() => new(
        BrokerId:        Guid.NewGuid(),
        ClientId:        Guid.NewGuid(),
        InstrumentId:    Guid.NewGuid(),
        Side:            TradeSide.Buy,
        Quantity:        100,
        Price:           250.50m,
        TradeDate:       DateTime.Today,
        SettlementNo:    "SN001",
        Source:          TradeSource.Manual,
        ExchangeTradeNo: null,
        SourceReference: null
    );

    [Fact]
    public void Validate_WhenValid_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenBrokerIdEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { BrokerId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.BrokerId);
    }

    [Fact]
    public void Validate_WhenClientIdEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { ClientId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.ClientId);
    }

    [Fact]
    public void Validate_WhenInstrumentIdEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { InstrumentId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.InstrumentId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenQuantityNotPositive_ShouldFail(int qty)
    {
        var result = _validator.TestValidate(ValidCommand() with { Quantity = qty });
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenPriceNotPositive_ShouldFail(int price)
    {
        var result = _validator.TestValidate(ValidCommand() with { Price = price });
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Validate_WhenSettlementNoEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { SettlementNo = "" });
        result.ShouldHaveValidationErrorFor(x => x.SettlementNo);
    }
}
