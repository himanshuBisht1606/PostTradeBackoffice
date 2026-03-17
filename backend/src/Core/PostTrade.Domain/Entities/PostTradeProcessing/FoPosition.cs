namespace PostTrade.Domain.Entities.PostTradeProcessing;

public class FoPosition : BaseEntity
{
    public Guid PositionRowId { get; set; }
    public Guid BatchId { get; set; }
    public Guid TenantId { get; set; }

    public DateOnly TradDt { get; set; }
    public string Sgmt { get; set; } = string.Empty;
    public string Src { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;   // NFO (NCL clears both)
    public string ClrMmbId { get; set; } = string.Empty;
    public string TradngMmbId { get; set; } = string.Empty; // BrkrOrCtdnPtcptId
    public string ClntTp { get; set; } = string.Empty;
    public string ClntId { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }

    // Instrument
    public string FinInstrmTp { get; set; } = string.Empty;
    public string Isin { get; set; } = string.Empty;
    public string TckrSymb { get; set; } = string.Empty;
    public string? XpryDt { get; set; }
    public DateOnly? ExpiryDate { get; set; }               // Parsed from XpryDt
    public decimal StrkPric { get; set; }
    public string OptnTp { get; set; } = string.Empty;
    public long NewBrdLotQty { get; set; }
    public string? ClientName { get; set; }
    public string? ClientStateCode { get; set; }            // For GST calculation

    // Long/short positions
    public long OpngLngQty { get; set; }
    public decimal OpngLngVal { get; set; }
    public long OpngShrtQty { get; set; }
    public decimal OpngShrtVal { get; set; }
    public long OpnBuyTradgQty { get; set; }
    public decimal OpnBuyTradgVal { get; set; }
    public long OpnSellTradgQty { get; set; }
    public decimal OpnSellTradgVal { get; set; }

    // Exercise/assignment
    public long PreExrcAssgndLngQty { get; set; }
    public decimal PreExrcAssgndLngVal { get; set; }
    public long PreExrcAssgndShrtQty { get; set; }
    public decimal PreExrcAssgndShrtVal { get; set; }
    public long ExrcdQty { get; set; }
    public long AssgndQty { get; set; }
    public long PstExrcAssgndLngQty { get; set; }
    public decimal PstExrcAssgndLngVal { get; set; }
    public long PstExrcAssgndShrtQty { get; set; }
    public decimal PstExrcAssgndShrtVal { get; set; }

    // Settlement
    public decimal SttlmPric { get; set; }
    public decimal RefRate { get; set; }
    public decimal PrmAmt { get; set; }
    public decimal DalyMrkToMktSettlmVal { get; set; }
    public decimal FutrsFnlSttlmVal { get; set; }
    public decimal ExrcAssgndVal { get; set; }

    public virtual FoFileImportBatch Batch { get; set; } = null!;
}
