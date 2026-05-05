using PostTrade.Domain.Enums;

namespace PostTrade.Domain.Entities.MasterData;

public class ExchangeSegment : BaseEntity
{
    public Guid ExchangeSegmentId { get; set; }
    public Guid TenantId { get; set; }
    public Guid ExchangeId { get; set; }
    public Guid SegmentId { get; set; }
    public string ExchangeSegmentCode { get; set; } = string.Empty;
    public string ExchangeSegmentName { get; set; } = string.Empty;
    public SettlementType SettlementType { get; set; }
    public bool IsActive { get; set; }

    // ── IntraOp / Clearing configuration (equivalent of CFORise SEGMENT_PARA) ──────────────
    /// <summary>
    /// Clearing corporation for this segment: "NCL" (NSCCL) or "ICCL".
    /// Determines GlobalExchangeCode — NCL→NFO billing, ICCL→BFO billing.
    /// </summary>
    public string? ClearingCorp { get; set; }           // NCL | ICCL

    /// <summary>
    /// The exchange used for billing / contract notes regardless of physical exchange.
    /// NFO if cleared by NSCCL (NCL); BFO if cleared by ICCL.
    /// Null = same as ExchangeSegmentCode (no interop).
    /// </summary>
    public string? GlobalExchangeCode { get; set; }     // NFO | BFO

    /// <summary>Broker's own Trading Member ID on this segment (TMID / Brkr field).</summary>
    public string? TradingMemberId { get; set; }

    /// <summary>Broker's contra / reversal code used on own trades (REVERSAL_CODE in CFORise).</summary>
    public string? BrokerCode { get; set; }

    // Navigation
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Exchange Exchange { get; set; } = null!;
    public virtual Segment Segment { get; set; } = null!;
    public virtual ICollection<ClientSegmentActivation> ClientSegmentActivations { get; set; } = new List<ClientSegmentActivation>();
}
