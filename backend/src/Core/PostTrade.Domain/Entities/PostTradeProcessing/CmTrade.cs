namespace PostTrade.Domain.Entities.PostTradeProcessing;

public class CmTrade : BaseEntity
{
    public Guid TradeRowId { get; set; }
    public Guid BatchId { get; set; }
    public Guid TenantId { get; set; }

    // File-parsed fields (NSE/BSE Trade file columns)
    public string UniqueTradeId { get; set; } = string.Empty;
    public DateOnly TradDt { get; set; }
    public string Sgmt { get; set; } = string.Empty;
    public string Src { get; set; } = string.Empty;
    public string FinInstrmId { get; set; } = string.Empty;
    public string FinInstrmNm { get; set; } = string.Empty;
    public string TradngMmbId { get; set; } = string.Empty;
    public string ClntId { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }
    public string OrdId { get; set; } = string.Empty;
    public string BuySellInd { get; set; } = string.Empty;
    public long TradQty { get; set; }
    public decimal PricePrUnit { get; set; }
    public decimal TradVal { get; set; }
    public string SttlmId { get; set; } = string.Empty;
    public string SttlmTyp { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;

    public virtual CmFileImportBatch Batch { get; set; } = null!;
}
