using PostTrade.Domain.Enums;

namespace PostTrade.Domain.Entities.MasterData;

/// <summary>
/// Captures the broker's membership for a specific Exchange-Segment combination.
/// TradingMemberId and ClearingMemberId are used during trade book processing and
/// ledger/obligation generation to identify the broker on the exchange.
/// </summary>
public class BrokerExchangeMembership : BaseAuditableEntity
{
    public Guid BrokerExchangeMembershipId { get; set; }
    public Guid BrokerId { get; set; }
    public Guid TenantId { get; set; }
    public Guid ExchangeSegmentId { get; set; }

    public string TradingMemberId { get; set; } = string.Empty;   // e.g. "12345" (NSE TM ID)
    public string? ClearingMemberId { get; set; }                  // e.g. "NCL12345"
    public MembershipType MembershipType { get; set; }

    public DateOnly EffectiveDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual Broker Broker { get; set; } = null!;
    public virtual ExchangeSegment ExchangeSegment { get; set; } = null!;
}
