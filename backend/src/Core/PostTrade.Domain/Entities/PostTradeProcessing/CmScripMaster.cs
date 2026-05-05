namespace PostTrade.Domain.Entities.PostTradeProcessing;

public class CmScripMaster : BaseEntity
{
    public Guid CmScripMasterId { get; set; }
    public Guid TenantId { get; set; }
    public string Exchange { get; set; } = string.Empty;
    public DateOnly TradingDate { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string ISIN { get; set; } = string.Empty;
    public string Series { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal FaceValue { get; set; }
    public int LotSize { get; set; }
    public decimal TickSize { get; set; }
    public string InstrumentType { get; set; } = string.Empty;
}
