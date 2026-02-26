using PostTrade.Domain.Enums;

namespace PostTrade.Domain.Entities.MasterData;

public class ClientSegmentActivation : BaseEntity
{
    public Guid ActivationId { get; set; }
    public Guid TenantId { get; set; }
    public Guid ClientId { get; set; }
    public Guid ExchangeSegmentId { get; set; }
    public ActivationStatus Status { get; set; }
    public decimal? ExposureLimit { get; set; }
    public MarginType MarginType { get; set; }
    public DateTime ActivatedOn { get; set; }
    public DateTime? DeactivatedOn { get; set; }

    // Navigation
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Client Client { get; set; } = null!;
    public virtual ExchangeSegment ExchangeSegment { get; set; } = null!;
}
