using PostTrade.Domain.Enums;

namespace PostTrade.Domain.Entities.Reconciliation;

public class Reconciliation : BaseAuditableEntity
{
    public Guid ReconId { get; set; }
    public Guid TenantId { get; set; }
    public DateTime ReconDate { get; set; }
    public string SettlementNo { get; set; } = string.Empty;
    public ReconType ReconType { get; set; }
    
    public decimal SystemValue { get; set; }
    public decimal ExchangeValue { get; set; }
    public decimal Difference { get; set; }
    public decimal ToleranceLimit { get; set; }
    
    public ReconStatus Status { get; set; }
    public string? Comments { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
}
