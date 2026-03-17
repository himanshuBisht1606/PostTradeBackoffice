namespace PostTrade.Domain.Entities.PostTradeProcessing;

public class FoTrade : BaseEntity
{
    public Guid TradeRowId { get; set; }
    public Guid BatchId { get; set; }
    public Guid TenantId { get; set; }

    // Instrument identification
    public string UniqueTradeId { get; set; } = string.Empty;  // UnqTradIdr
    public DateOnly TradDt { get; set; }
    public string Sgmt { get; set; } = string.Empty;           // FO
    public string Src { get; set; } = string.Empty;            // NSE | BSE
    public string Exchange { get; set; } = string.Empty;       // NFO | BFO
    public string TradngMmbId { get; set; } = string.Empty;    // Brkr
    public string FinInstrmTp { get; set; } = string.Empty;    // IDF/STF/IDO/STO
    public string FinInstrmId { get; set; } = string.Empty;
    public string Isin { get; set; } = string.Empty;
    public string TckrSymb { get; set; } = string.Empty;

    // F&O specific fields
    public string? XpryDt { get; set; }                        // Expiry date (raw string)
    public decimal StrkPric { get; set; }                      // Strike price
    public string OptnTp { get; set; } = string.Empty;         // CE | PE | blank(futures)
    public string FinInstrmNm { get; set; } = string.Empty;    // e.g. BANKNIFTY26MARFUT

    // Client
    public string ClntTp { get; set; } = string.Empty;         // C=Client, P=Prop
    public string ClntId { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }

    // Trade details
    public string BuySellInd { get; set; } = string.Empty;     // B | S
    public long TradQty { get; set; }
    public long NewBrdLotQty { get; set; }
    public decimal Pric { get; set; }
    public string SttlmTp { get; set; } = string.Empty;
    public string SctiesSttlmTxId { get; set; } = string.Empty;

    public virtual FoFileImportBatch Batch { get; set; } = null!;
}
