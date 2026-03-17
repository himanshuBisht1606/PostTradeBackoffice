namespace PostTrade.Domain.Entities.PostTradeProcessing;

public class FoStt : BaseEntity
{
    public Guid SttRowId { get; set; }
    public Guid BatchId { get; set; }
    public Guid TenantId { get; set; }

    public string RptHdr { get; set; } = string.Empty;     // 10|20|30 section marker
    public DateOnly TradDt { get; set; }
    public string Sgmt { get; set; } = string.Empty;
    public string Src { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;   // NFO (NCL clears both)
    public string ClrMmbId { get; set; } = string.Empty;
    public string TradngMmbId { get; set; } = string.Empty; // BrkrOrCtdnPtcptId
    public string ClntId { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }

    // Instrument
    public string TckrSymb { get; set; } = string.Empty;
    public string FinInstrmTp { get; set; } = string.Empty;
    public string Isin { get; set; } = string.Empty;
    public string? XpryDt { get; set; }
    public string OptnTp { get; set; } = string.Empty;
    public decimal StrkPric { get; set; }
    public decimal SttlmPric { get; set; }

    // Volumes
    public long TtlBuyTradgVol { get; set; }
    public decimal TtlBuyTrfVal { get; set; }
    public long TtlSellTradgVol { get; set; }
    public decimal TtlSellTrfVal { get; set; }

    // FO-specific tax fields
    public decimal TaxblSellFutrsVal { get; set; }
    public decimal TaxblSellOptnVal { get; set; }
    public long OptnExrcQty { get; set; }
    public decimal OptnExrcVal { get; set; }
    public decimal TaxblExrcVal { get; set; }
    public decimal FutrsTtlTaxs { get; set; }
    public decimal OptnTtlTaxs { get; set; }
    public decimal TtlTaxs { get; set; }

    public virtual FoFileImportBatch Batch { get; set; } = null!;
}
