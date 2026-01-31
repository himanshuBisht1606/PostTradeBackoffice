namespace PostTrade.Domain.Entities.MasterData;

public class Segment : BaseEntity
{
    public Guid SegmentId { get; set; }
    public Guid TenantId { get; set; }
    public Guid ExchangeId { get; set; }
    public string SegmentCode { get; set; } = string.Empty;
    public string SegmentName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    
    // Navigation
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Exchange Exchange { get; set; } = null!;
}
