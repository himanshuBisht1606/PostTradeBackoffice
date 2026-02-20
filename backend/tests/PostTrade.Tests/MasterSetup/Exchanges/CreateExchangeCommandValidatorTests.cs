using FluentValidation.TestHelper;
using PostTrade.Application.Features.MasterSetup.Exchanges.Commands;

namespace PostTrade.Tests.MasterSetup.Exchanges;

public class CreateExchangeCommandValidatorTests
{
    private readonly CreateExchangeCommandValidator _validator = new();

    private static CreateExchangeCommand ValidCommand() => new(
        ExchangeCode:     "NSE",
        ExchangeName:     "National Stock Exchange",
        Country:          "India",
        TimeZone:         null,
        TradingStartTime: null,
        TradingEndTime:   null
    );

    [Fact]
    public void Validate_WhenValid_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenExchangeCodeEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { ExchangeCode = "" });
        result.ShouldHaveValidationErrorFor(x => x.ExchangeCode);
    }

    [Fact]
    public void Validate_WhenExchangeNameEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { ExchangeName = "" });
        result.ShouldHaveValidationErrorFor(x => x.ExchangeName);
    }

    [Fact]
    public void Validate_WhenCountryEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { Country = "" });
        result.ShouldHaveValidationErrorFor(x => x.Country);
    }
}
