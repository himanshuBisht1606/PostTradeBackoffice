namespace PostTrade.Domain.Entities.PostTradeProcessing;

/// <summary>
/// Structured FO client position book — populated from FoPositions (staging) during import.
/// </summary>
public class FoClientPositionBook : BaseEntity
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public Guid TenantId { get; set; }

    public DateOnly TradeDate { get; set; }
    public string Exchange { get; set; } = string.Empty;
    public string? SegmentIndicator { get; set; }               // Segment type indicator (SEG_IND)
    public string ClearingMemberId { get; set; } = string.Empty;
    public string BrokerId { get; set; } = string.Empty;

    // Client
    public string ClientType { get; set; } = string.Empty;     // C = Client, P = Proprietary
    public string ClientCode { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? ClientStateCode { get; set; }

    // Instrument
    public string Symbol { get; set; } = string.Empty;
    public string? ContractName { get; set; }                   // Full contract name e.g. NIFTY26MARFUT (CONTNAME)
    public string ContractType { get; set; } = string.Empty;   // Index Future / Stock Future / Index Option / Stock Option
    public string Isin { get; set; } = string.Empty;
    public DateOnly? ExpiryDate { get; set; }
    public decimal StrikePrice { get; set; }
    public string OptionType { get; set; } = string.Empty;     // CE | PE | FX (futures)
    public long LotSize { get; set; }

    // Opening positions (carried forward)
    public long OpenLongQty { get; set; }
    public decimal OpenLongValue { get; set; }
    public long OpenShortQty { get; set; }
    public decimal OpenShortValue { get; set; }

    // Day trading
    public long DayBuyQty { get; set; }
    public decimal DayBuyValue { get; set; }
    public long DaySellQty { get; set; }
    public decimal DaySellValue { get; set; }

    // Pre-exercise / assignment
    public long PreExerciseLongQty { get; set; }
    public decimal PreExerciseLongValue { get; set; }
    public long PreExerciseShortQty { get; set; }
    public decimal PreExerciseShortValue { get; set; }

    // Exercise / assignment outcome
    public long ExercisedQty { get; set; }
    public long AssignedQty { get; set; }
    public long PostExerciseLongQty { get; set; }
    public decimal PostExerciseLongValue { get; set; }
    public long PostExerciseShortQty { get; set; }
    public decimal PostExerciseShortValue { get; set; }

    // Settlement
    public decimal SettlementPrice { get; set; }
    public decimal ReferenceRate { get; set; }
    public decimal PremiumAmount { get; set; }
    public decimal NetPremium { get; set; }                     // Net premium paid/received (NETPREM)
    public decimal DailyMtmSettlement { get; set; }
    public decimal FuturesFinalSettlement { get; set; }
    public decimal ExerciseAssignmentValue { get; set; }

    public virtual FoFileImportBatch Batch { get; set; } = null!;
}
