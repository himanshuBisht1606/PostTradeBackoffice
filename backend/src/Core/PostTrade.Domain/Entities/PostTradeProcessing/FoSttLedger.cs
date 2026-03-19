namespace PostTrade.Domain.Entities.PostTradeProcessing;

/// <summary>
/// Structured FO Securities Transaction Tax ledger — populated from FoStts (staging) during import.
/// Only client-level rows (RptHdr = 30) are stored.
/// </summary>
public class FoSttLedger : BaseEntity
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public Guid TenantId { get; set; }

    public DateOnly TradeDate { get; set; }
    public string Exchange { get; set; } = string.Empty;
    public string ClearingMemberId { get; set; } = string.Empty;
    public string BrokerId { get; set; } = string.Empty;

    // Client
    public string ClientCode { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? ClientStateCode { get; set; }

    // Instrument
    public string Symbol { get; set; } = string.Empty;
    public string ContractType { get; set; } = string.Empty;   // Index Future / Stock Future / Index Option / Stock Option
    public string Isin { get; set; } = string.Empty;
    public DateOnly? ExpiryDate { get; set; }
    public decimal StrikePrice { get; set; }
    public string OptionType { get; set; } = string.Empty;     // CE | PE | FX (futures)
    public decimal SettlementPrice { get; set; }

    // Trading volumes
    public long TotalBuyQty { get; set; }
    public decimal TotalBuyValue { get; set; }
    public long TotalSellQty { get; set; }
    public decimal TotalSellValue { get; set; }

    // Taxable values
    public decimal TaxableSellFuturesValue { get; set; }
    public decimal TaxableSellOptionValue { get; set; }
    public long OptionExerciseQty { get; set; }
    public decimal OptionExerciseValue { get; set; }
    public decimal TaxableExerciseValue { get; set; }

    // STT amounts
    public decimal FuturesStt { get; set; }                     // STT on futures sell (STT_FUTURES)
    public decimal OptionsStt { get; set; }                     // STT on options sell premium (STT_OPTION)
    public decimal FuturesExpiryStt { get; set; }               // STT on futures expiry settlement (STT_FUTEXP)
    public decimal OptionsExpiryStt { get; set; }               // STT on options exercise/expiry (STT_OPEXP)
    public decimal TotalStt { get; set; }

    public virtual FoFileImportBatch Batch { get; set; } = null!;
}
