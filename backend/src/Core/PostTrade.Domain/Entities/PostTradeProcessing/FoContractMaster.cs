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
    public DateOnly? ExpiryDate { get; set; }               // Parsed from XpryDt
    public decimal StrkPric { get; set; }
    public string OptnTp { get; set; } = string.Empty;     // CE | PE | blank (futures)
    public string FinInstrmTp { get; set; } = string.Empty; // IDF/STF/IDO/STO
    public string SttlmMtd { get; set; } = string.Empty;
    public string StockNm { get; set; } = string.Empty;

    // Lot size
    public long MinLot { get; set; }
    public long NewBrdLotQty { get; set; }

    // Additional contract attributes
    public decimal TickSize { get; set; }               // Bid interval / tick size (BidIntrvl f[10])
    public decimal? BasePric { get; set; }              // Base/reference price (NSE: f[20])
    public string? MktTpAndId { get; set; }             // Market type and ID (f[27])
    public string? OptnExrcStyle { get; set; }          // E=European / A=American (f[60])
    public string? Isin { get; set; }                   // ISIN code (f[110])

    /// <summary>
    /// Price multiplier (CMULTIPLIER in CFORise) — from Mltplr column (f[71] in NFO/BFO contract file).
    /// Used for TradeValue = Qty × Price × FMultiplier. Default 1 for equity derivatives.
    /// </summary>
    public decimal FMultiplier { get; set; } = 1m;

    // Instrument registration — set when this contract is promoted to a master Instrument
    public Guid? RegisteredInstrumentId { get; set; }

    public virtual FoFileImportBatch Batch { get; set; } = null!;
}
