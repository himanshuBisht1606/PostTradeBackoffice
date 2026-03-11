namespace PostTrade.Domain.Entities.PostTradeProcessing;

public class CmObligation : BaseEntity
{
    public Guid ObligationRowId { get; set; }
    public Guid BatchId { get; set; }
    public Guid TenantId { get; set; }

    // File-parsed fields (NCL Obligation file columns)
    public DateOnly TradDt { get; set; }
    public string TradngMmbId { get; set; } = string.Empty;
    public string ClntId { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }
    public string SttlmId { get; set; } = string.Empty;
    public DateOnly SttlmDt { get; set; }
    public string IsinCode { get; set; } = string.Empty;
    public string ScripNm { get; set; } = string.Empty;
    public string ObligTyp { get; set; } = string.Empty;
    public long NetQty { get; set; }
    public decimal ObligStdAmt { get; set; }
    public decimal CrObligStdAmt { get; set; }
    public decimal DrObligStdAmt { get; set; }

    public virtual CmFileImportBatch Batch { get; set; } = null!;
}
