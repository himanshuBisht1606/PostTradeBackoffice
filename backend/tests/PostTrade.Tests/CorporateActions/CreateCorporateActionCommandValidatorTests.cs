using FluentValidation.TestHelper;
using PostTrade.Application.Features.CorporateActions.Commands;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.CorporateActions;

public class CreateCorporateActionCommandValidatorTests
{
    private readonly CreateCorporateActionCommandValidator _validator = new();

    private static CreateCorporateActionCommand ValidDividendCommand() => new(
        InstrumentId:     Guid.NewGuid(),
        ActionType:       CorporateActionType.Dividend,
        AnnouncementDate: DateTime.Today,
        ExDate:           DateTime.Today.AddDays(5),
        RecordDate:       DateTime.Today.AddDays(7),
        PaymentDate:      DateTime.Today.AddDays(10),
        DividendAmount:   2.50m,
        BonusRatio:       null,
        SplitRatio:       null,
        RightsRatio:      null,
        RightsPrice:      null
    );

    [Fact]
    public void Validate_WhenValid_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(ValidDividendCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenInstrumentIdEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidDividendCommand() with { InstrumentId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.InstrumentId);
    }

    [Fact]
    public void Validate_WhenExDateBeforeAnnouncementDate_ShouldFail()
    {
        var cmd    = ValidDividendCommand() with { ExDate = DateTime.Today.AddDays(-1) };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ExDate);
    }

    [Fact]
    public void Validate_WhenRecordDateBeforeExDate_ShouldFail()
    {
        var cmd    = ValidDividendCommand() with { RecordDate = DateTime.Today.AddDays(1) }; // ExDate is +5
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.RecordDate);
    }

    [Fact]
    public void Validate_WhenDividendAction_AndDividendAmountIsZero_ShouldFail()
    {
        // GreaterThan(0) skips null in FluentValidation 11 — use 0 which is definitively not > 0
        var cmd    = ValidDividendCommand() with { DividendAmount = 0m };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.DividendAmount);
    }

    [Fact]
    public void Validate_WhenBonusAction_AndBonusRatioIsZero_ShouldFail()
    {
        var cmd = new CreateCorporateActionCommand(
            InstrumentId:     Guid.NewGuid(),
            ActionType:       CorporateActionType.Bonus,
            AnnouncementDate: DateTime.Today,
            ExDate:           DateTime.Today.AddDays(5),
            RecordDate:       DateTime.Today.AddDays(7),
            PaymentDate:      null,
            DividendAmount:   null,
            BonusRatio:       0m, // not > 0 → fails
            SplitRatio:       null,
            RightsRatio:      null,
            RightsPrice:      null
        );
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.BonusRatio);
    }
}
