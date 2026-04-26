namespace PostTrade.Domain.Entities.PostTradeProcessing;

/// <summary>
/// Computed per-client FO Finance Ledger for a trade date and exchange.
/// Aggregates turnover (FoTradeBook), STT (FoSttLedger), Stamp Duty (FoStampDutyLedger),
/// and settlement (FoClientPositionBook) into a single client-level summary.
/// Broker-computed charges (Brokerage, Exchange charges, SEBI, IPFT, GST) default to zero
/// until the Contract Charges step is implemented.
/// </summary>
public class FoFinanceLedger : BaseEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    public DateOnly TradeDate { get; set; }
    public string Exchange { get; set; } = string.Empty;
    public string ClearingMemberId { get; set; } = string.Empty;
    public string BrokerId { get; set; } = string.Empty;

    // Client
    public string ClientCode { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }

    // Turnover (from FoTradeBook)
    public decimal BuyTurnover { get; set; }
    public decimal SellTurnover { get; set; }
    public decimal TotalTurnover { get; set; }

    // Exchange-provided charges (from FoSttLedger, FoStampDutyLedger)
    public decimal TotalStt { get; set; }
    public decimal TotalStampDuty { get; set; }

    // Broker-computed charges — populated in Contract Charges step (step 4)
    public decimal Brokerage { get; set; }
    public decimal ExchangeTransactionCharges { get; set; }
    public decimal SebiCharges { get; set; }
    public decimal Ipft { get; set; }
    public decimal GstOnCharges { get; set; }

    /// <summary>Sum of all charges: STT + StampDuty + Brokerage + Exchange + SEBI + IPFT + GST.</summary>
    public decimal TotalCharges { get; set; }

    // Settlement (from FoClientPositionBook)
    public decimal DailyMtmSettlement { get; set; }
    public decimal NetPremium { get; set; }
    public decimal FinalSettlement { get; set; }
    public decimal ExerciseAssignmentValue { get; set; }

    /// <summary>
    /// Net amount = (DailyMtmSettlement + NetPremium + FinalSettlement + ExerciseAssignmentValue) - TotalCharges.
    /// Positive = client receives (pay-out from broker); Negative = client owes (pay-in to broker).
    /// </summary>
    public decimal NetAmount { get; set; }
}
