namespace PostTrade.Domain.Entities.PostTradeProcessing;

public class CmMargin : BaseEntity
{
    public Guid MarginRowId { get; set; }
    public Guid BatchId { get; set; }
    public Guid TenantId { get; set; }

    // File-parsed fields (NCL Margin file columns)
    public DateOnly TradDt { get; set; }
    public string TradngMmbId { get; set; } = string.Empty;
    public string ClntId { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }
    public string Sgmt { get; set; } = string.Empty;
    public string IsinCode { get; set; } = string.Empty;
    public string ScripNm { get; set; } = string.Empty;
    public decimal MtmMrgnAmt { get; set; }
    public decimal VrMrgnAmt { get; set; }
    public decimal ExpsrMrgnAmt { get; set; }
    public decimal AddhcMrgnAmt { get; set; }
    public decimal CrystldLssAmt { get; set; }
    public decimal TtlMrgnAmt { get; set; }

    public virtual CmFileImportBatch Batch { get; set; } = null!;
}
