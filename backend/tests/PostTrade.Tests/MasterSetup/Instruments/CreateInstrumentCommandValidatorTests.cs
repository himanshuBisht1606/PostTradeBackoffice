using FluentValidation.TestHelper;
using PostTrade.Application.Features.MasterSetup.Instruments.Commands;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.MasterSetup.Instruments;

public class CreateInstrumentCommandValidatorTests
{
    private readonly CreateInstrumentCommandValidator _validator = new();

    private static CreateInstrumentCommand ValidCommand() => new(
        InstrumentCode: "RELIANCE",
        InstrumentName: "Reliance Industries",
        Symbol:         "RELIANCE",
        ISIN:           null,
        ExchangeId:     Guid.NewGuid(),
        SegmentId:      Guid.NewGuid(),
        InstrumentType: InstrumentType.Equity,
        LotSize:        1m,
        TickSize:       0.05m,
        Series:         null,
        ExpiryDate:     null,
        StrikePrice:    null,
        OptionType:     null
    );

    [Fact]
    public void Validate_WhenValid_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenInstrumentCodeEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { InstrumentCode = "" });
        result.ShouldHaveValidationErrorFor(x => x.InstrumentCode);
    }

    [Fact]
    public void Validate_WhenInstrumentNameEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { InstrumentName = "" });
        result.ShouldHaveValidationErrorFor(x => x.InstrumentName);
    }

    [Fact]
    public void Validate_WhenSymbolEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { Symbol = "" });
        result.ShouldHaveValidationErrorFor(x => x.Symbol);
    }

    [Fact]
    public void Validate_WhenExchangeIdEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { ExchangeId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.ExchangeId);
    }

    [Fact]
    public void Validate_WhenSegmentIdEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { SegmentId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.SegmentId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenLotSizeNotPositive_ShouldFail(decimal lotSize)
    {
        var result = _validator.TestValidate(ValidCommand() with { LotSize = lotSize });
        result.ShouldHaveValidationErrorFor(x => x.LotSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    public void Validate_WhenTickSizeNotPositive_ShouldFail(decimal tickSize)
    {
        var result = _validator.TestValidate(ValidCommand() with { TickSize = tickSize });
        result.ShouldHaveValidationErrorFor(x => x.TickSize);
    }
}
