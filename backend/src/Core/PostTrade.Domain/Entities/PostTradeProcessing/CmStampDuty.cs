namespace PostTrade.Domain.Entities.PostTradeProcessing;

public class CmStampDuty : BaseEntity
{
    public Guid StampDutyRowId { get; set; }
    public Guid BatchId { get; set; }
    public Guid TenantId { get; set; }

    // File-parsed fields (NCL StampDuty file columns — all RptHdr levels)
    public string RptHdr { get; set; } = string.Empty;
    public DateOnly TradDt { get; set; }
    public string TradngMmbId { get; set; } = string.Empty;
    public string ClntId { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }
    public string Sgmt { get; set; } = string.Empty;
    public string IsinCode { get; set; } = string.Empty;
    public string ScripNm { get; set; } = string.Empty;
    public string BuySellInd { get; set; } = string.Empty;
    public long TradQty { get; set; }
    public decimal TradVal { get; set; }
    public decimal StmpDtyAmt { get; set; }
    public decimal StmpDtyRate { get; set; }

    public virtual CmFileImportBatch Batch { get; set; } = null!;
}
