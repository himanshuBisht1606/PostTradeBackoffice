using PostTrade.Domain.Enums;

namespace PostTrade.Domain.Entities.CorporateActions;

public class CorporateAction : BaseAuditableEntity
{
    public Guid CorporateActionId { get; set; }
    public Guid TenantId { get; set; }
    public Guid InstrumentId { get; set; }
    public CorporateActionType ActionType { get; set; }
    public DateTime AnnouncementDate { get; set; }
    public DateTime ExDate { get; set; }
    public DateTime RecordDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    
    // Action specific fields
    public decimal? DividendAmount { get; set; }
    public decimal? BonusRatio { get; set; }
    public decimal? SplitRatio { get; set; }
    public decimal? RightsRatio { get; set; }
    public decimal? RightsPrice { get; set; }
    
    public CorporateActionStatus Status { get; set; }
    public bool IsProcessed { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
