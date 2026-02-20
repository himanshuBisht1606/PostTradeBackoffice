using FluentValidation.TestHelper;
using PostTrade.Application.Features.Ledger.Entries.Commands;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.Ledger;

public class PostLedgerEntryCommandValidatorTests
{
    private readonly PostLedgerEntryCommandValidator _validator = new();

    private static PostLedgerEntryCommand ValidCommand() => new(
        BrokerId:      Guid.NewGuid(),
        ClientId:      Guid.NewGuid(),
        VoucherNo:     "VCH001",
        PostingDate:   DateTime.Today,
        ValueDate:     DateTime.Today,
        LedgerType:    LedgerType.ClientLedger,
        EntryType:     EntryType.Trade,
        Debit:         0m,
        Credit:        5000m,
        ReferenceType: "Trade",
        ReferenceId:   Guid.NewGuid(),
        Narration:     null
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
    public void Validate_WhenVoucherNoEmpty_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { VoucherNo = "" });
        result.ShouldHaveValidationErrorFor(x => x.VoucherNo);
    }

    [Fact]
    public void Validate_WhenDebitAndCreditBothZero_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { Debit = 0m, Credit = 0m });
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validate_WhenDebitIsNegative_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { Debit = -1m, Credit = 0m });
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validate_WhenCreditIsNegative_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { Debit = 0m, Credit = -1m });
        result.ShouldHaveValidationErrorFor(x => x);
    }
}
