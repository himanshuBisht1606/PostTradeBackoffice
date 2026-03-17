namespace PostTrade.Domain.Entities.PostTradeProcessing;

public class FoStampDuty : BaseEntity
{
    public Guid StampDutyRowId { get; set; }
    public Guid BatchId { get; set; }
    public Guid TenantId { get; set; }

    public string RptHdr { get; set; } = string.Empty;
    public DateOnly TradDt { get; set; }
    public string Sgmt { get; set; } = string.Empty;
    public string Src { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public string ClrMmbId { get; set; } = string.Empty;
    public string TradngMmbId { get; set; } = string.Empty; // BrkrOrCtdnPtcptId
    public string ClntId { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }
    public string CtrySubDvsn { get; set; } = string.Empty; // State

    // Instrument
    public string TckrSymb { get; set; } = string.Empty;
    public string FinInstrmTp { get; set; } = string.Empty;
    public string Isin { get; set; } = string.Empty;
    public string? XpryDt { get; set; }
    public decimal StrkPric { get; set; }
    public string OptnTp { get; set; } = string.Empty;

    // Volumes & values
    public long TtlBuyTradgVol { get; set; }
    public decimal TtlBuyTrfVal { get; set; }
    public long TtlSellTradgVol { get; set; }
    public decimal TtlSellTrfVal { get; set; }
    public long BuyDlvryQty { get; set; }
    public decimal BuyDlvryVal { get; set; }
    public long BuyOthrThanDlvryQty { get; set; }
    public decimal BuyOthrThanDlvryVal { get; set; }

    // Stamp duty amounts
    public decimal BuyStmpDty { get; set; }
    public decimal SellStmpDty { get; set; }
    public decimal SttlmPric { get; set; }
    public decimal BuyDlvryStmpDty { get; set; }
    public decimal BuyOthrThanDlvryStmpDty { get; set; }
    public decimal StmpDtyAmt { get; set; }

    public virtual FoFileImportBatch Batch { get; set; } = null!;
}
