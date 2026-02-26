using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.Ledger.Entries.DTOs;

public record LedgerEntryDto(
    Guid LedgerId,
    Guid TenantId,
    Guid BrokerId,
    Guid ClientId,
    string VoucherNo,
    DateTime PostingDate,
    DateTime ValueDate,
    LedgerType LedgerType,
    EntryType EntryType,
    decimal Debit,
    decimal Credit,
    decimal Balance,
    string ReferenceType,
    Guid ReferenceId,
    string? Narration,
    bool IsReversed,
    Guid? ReversalLedgerId
);
