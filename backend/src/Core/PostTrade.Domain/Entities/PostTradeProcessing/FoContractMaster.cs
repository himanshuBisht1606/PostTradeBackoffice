namespace PostTrade.Domain.Entities.PostTradeProcessing;

public class FoContractMaster : BaseEntity
{
    public Guid ContractRowId { get; set; }
    public Guid BatchId { get; set; }
    public Guid TenantId { get; set; }

    public DateOnly TradingDate { get; set; }
    public string Exchange { get; set; } = string.Empty;    // NFO | BFO

    // Key identification fields
    public string FinInstrmId { get; set; } = string.Empty;
    public string UndrlygFinInstrmId { get; set; } = string.Empty;
    public string FinInstrmNm { get; set; } = string.Empty;
    public string TckrSymb { get; set; } = string.Empty;

    // F&O attributes
    public string XpryDt { get; set; } = string.Empty;     // Raw string (epoch or DD-MON-YYYY)
    public decimal StrkPric { get; set; }
    public string OptnTp { get; set; } = string.Empty;     // CE | PE | blank (futures)
    public string FinInstrmTp { get; set; } = string.Empty; // IDF/STF/IDO/STO
    public string SttlmMtd { get; set; } = string.Empty;
    public string StockNm { get; set; } = string.Empty;

    // Lot size
    public long MinLot { get; set; }
    public long NewBrdLotQty { get; set; }

    public virtual FoFileImportBatch Batch { get; set; } = null!;
}
