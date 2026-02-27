using FluentValidation.TestHelper;
using PostTrade.Application.Features.Trading.Trades.Commands;

namespace PostTrade.Tests.Trading.Trades;

public class CancelTradeCommandValidatorTests
{
    private readonly CancelTradeCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenValid_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(new CancelTradeCommand(Guid.NewGuid(), "Client request"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenTradeIdEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(new CancelTradeCommand(Guid.Empty, "reason"));
        result.ShouldHaveValidationErrorFor(x => x.TradeId);
    }

    [Fact]
    public void Validate_WhenReasonEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(new CancelTradeCommand(Guid.NewGuid(), ""));
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }
}
