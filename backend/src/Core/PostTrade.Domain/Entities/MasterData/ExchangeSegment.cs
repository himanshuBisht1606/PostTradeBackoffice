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

    // Navigation
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Exchange Exchange { get; set; } = null!;
    public virtual Segment Segment { get; set; } = null!;
    public virtual ICollection<ClientSegmentActivation> ClientSegmentActivations { get; set; } = new List<ClientSegmentActivation>();
}
