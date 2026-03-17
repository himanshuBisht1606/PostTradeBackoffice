namespace PostTrade.Domain.Entities.PostTradeProcessing;

public class FoBhavCopy : BaseEntity
{
    public Guid BhavCopyRowId { get; set; }
    public Guid BatchId { get; set; }
    public Guid TenantId { get; set; }

    public DateOnly TradDt { get; set; }
    public string Exchange { get; set; } = string.Empty;   // NFO | BFO
    public string Sgmt { get; set; } = string.Empty;
    public string Src { get; set; } = string.Empty;
    public string FinInstrmTp { get; set; } = string.Empty; // IDF/STF/IDO/STO
    public string FinInstrmId { get; set; } = string.Empty;
    public string Isin { get; set; } = string.Empty;
    public string TckrSymb { get; set; } = string.Empty;
    public string SctySrs { get; set; } = string.Empty;
    public string? XpryDt { get; set; }
    public decimal StrkPric { get; set; }
    public string OptnTp { get; set; } = string.Empty;      // CE | PE | blank
    public string FinInstrmNm { get; set; } = string.Empty;

    // OHLC + settlement
    public decimal OpnPric { get; set; }
    public decimal HghPric { get; set; }
    public decimal LwPric { get; set; }
    public decimal ClsPric { get; set; }
    public decimal LastPric { get; set; }
    public decimal PrvsClsgPric { get; set; }
    public decimal UndrlygPric { get; set; }
    public decimal SttlmPric { get; set; }

    // Open interest
    public long OpnIntrst { get; set; }
    public long ChngInOpnIntrst { get; set; }

    // Volume
    public long TtlTradgVol { get; set; }
    public decimal TtlTrfVal { get; set; }
    public long TtlNbOfTxsExctd { get; set; }
    public long NewBrdLotQty { get; set; }

    public virtual FoFileImportBatch Batch { get; set; } = null!;
}
