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

    // F&O enriched fields
    public DateOnly? ExpiryDate { get; set; }                   // Parsed from XpryDt
    public string InstrumentType { get; set; } = string.Empty;  // Index Future/Stock Future/Index Option/Stock Option
    public long LotSize { get; set; }                           // From contract master
    public string UnderlyingSymbol { get; set; } = string.Empty; // Underlying (e.g. NIFTY, RELIANCE)

    // Client
    public string ClntTp { get; set; } = string.Empty;         // C=Client, P=Prop
    public string ClntId { get; set; } = string.Empty;
    public string? CtclId { get; set; }                        // Exchange unique client terminal ID
    public string? OrgnlCtdnPtcptId { get; set; }             // Original custodian participant ID (ORGCLENTID)
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? ClientStateCode { get; set; }                // For GST calculation

    // Trade details
    public string BuySellInd { get; set; } = string.Empty;     // B | S
    public long TradQty { get; set; }
    public long NewBrdLotQty { get; set; }
    public decimal Pric { get; set; }
    public string SttlmTp { get; set; } = string.Empty;
    public string SctiesSttlmTxId { get; set; } = string.Empty;

    // Additional exchange file fields
    public string? TradDtTm { get; set; }                      // Full trade timestamp (TradDtTm)
    public string? RptdTxSts { get; set; }                     // Reported transaction status: OR=Order, CN=Cancelled
    public string? OrdrRef { get; set; }                       // Exchange order reference (OrdrRef)
    public string? SttlmCycl { get; set; }                     // Settlement cycle (e.g. T+1)
    public string? MktTpandId { get; set; }                    // Market type and ID (MktTpandId)

    // Calculated values
    public decimal TradeValue { get; set; }                     // TradQty * Pric
    public decimal NumLots { get; set; }                        // TradQty / LotSize

    public virtual FoFileImportBatch Batch { get; set; } = null!;
}
