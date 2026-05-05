namespace PostTrade.Domain.Entities.PostTradeProcessing;

/// <summary>
/// Structured FO Stamp Duty ledger — populated from FoStampDuties (staging) during import.
/// Only client-level rows (RptHdr = 30) are stored.
/// </summary>
public class FoStampDutyLedger : BaseEntity
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
    public string StateCode { get; set; } = string.Empty;      // State code for stamp duty applicability (STATECODE)
    public string? StateName { get; set; }                      // State name (STATE)

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

    // Delivery breakdown
    public long DeliveryBuyQty { get; set; }
    public decimal DeliveryBuyValue { get; set; }
    public long NonDeliveryBuyQty { get; set; }
    public decimal NonDeliveryBuyValue { get; set; }

    // Stamp duty amounts
    public decimal BuyStampDuty { get; set; }                   // Intraday buy stamp duty (BuyStmpDty)
    public decimal SellStampDuty { get; set; }                  // Intraday sell stamp duty (SellStmpDty)
    public decimal FuturesStampDuty { get; set; }               // Futures stamp duty (STAMP_FUTURE)
    public decimal OptionsStampDuty { get; set; }               // Options stamp duty (STAMP_OPTION)
    public decimal FuturesExpiryStampDuty { get; set; }         // Futures expiry stamp duty (STAMP_FUTEXP)
    public decimal OptionsExpiryStampDuty { get; set; }         // Options expiry/exercise stamp duty (STAMP_OPEXP)
    public decimal DeliveryBuyStampDuty { get; set; }
    public decimal NonDeliveryBuyStampDuty { get; set; }
    public decimal TotalStampDuty { get; set; }

    public virtual FoFileImportBatch Batch { get; set; } = null!;
}
