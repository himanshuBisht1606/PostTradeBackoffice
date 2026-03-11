namespace PostTrade.Domain.Entities.PostTradeProcessing;

public class CmSettlementMaster : BaseEntity
{
    public Guid CmSettlementMasterId { get; set; }
    public Guid TenantId { get; set; }
    public string Exchange { get; set; } = string.Empty;
    public DateOnly TradingDate { get; set; }
    public string SettlementNo { get; set; } = string.Empty;
    public string SettlementType { get; set; } = string.Empty;
    public DateOnly PayInDate { get; set; }
    public DateOnly PayOutDate { get; set; }
}
