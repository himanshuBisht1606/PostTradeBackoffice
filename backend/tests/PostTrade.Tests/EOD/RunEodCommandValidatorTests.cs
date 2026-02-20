using FluentValidation.TestHelper;
using PostTrade.Application.Features.EOD.Commands;

namespace PostTrade.Tests.EOD;

public class RunEodCommandValidatorTests
{
    private readonly RunEodCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenDateIsToday_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(new RunEodCommand(DateTime.Today));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenDateIsInThePast_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(new RunEodCommand(DateTime.Today.AddDays(-1)));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenDateIsInTheFuture_ShouldFail()
    {
        var result = _validator.TestValidate(new RunEodCommand(DateTime.UtcNow.Date.AddDays(1)));
        result.ShouldHaveValidationErrorFor(x => x.TradingDate);
    }
}
