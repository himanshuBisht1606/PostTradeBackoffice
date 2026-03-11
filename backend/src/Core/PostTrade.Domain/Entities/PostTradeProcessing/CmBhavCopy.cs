namespace PostTrade.Domain.Entities.PostTradeProcessing;

public class CmBhavCopy : BaseEntity
{
    public Guid BhavCopyRowId { get; set; }
    public Guid BatchId { get; set; }
    public Guid TenantId { get; set; }

    // File-parsed fields (NSE/BSE BhavCopy file columns)
    public DateOnly TradDt { get; set; }
    public string FinInstrmId { get; set; } = string.Empty;
    public string FinInstrmNm { get; set; } = string.Empty;
    public string Isin { get; set; } = string.Empty;
    public string SctySrs { get; set; } = string.Empty;
    public decimal OpnPric { get; set; }
    public decimal HghPric { get; set; }
    public decimal LwPric { get; set; }
    public decimal ClsPric { get; set; }
    public decimal LastPric { get; set; }
    public decimal PrvClsgPric { get; set; }
    public long TtlTradgVol { get; set; }
    public decimal TtlTrfVal { get; set; }
    public decimal? MktCpzn { get; set; }
    public string Exchange { get; set; } = string.Empty;

    public virtual CmFileImportBatch Batch { get; set; } = null!;
}
