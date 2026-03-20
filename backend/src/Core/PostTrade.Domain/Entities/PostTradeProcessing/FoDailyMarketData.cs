namespace PostTrade.Domain.Entities.PostTradeProcessing;

/// <summary>
/// Structured FO end-of-day market data — populated from FoBhavCopies (staging) during import.
/// </summary>
public class FoDailyMarketData : BaseEntity
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public Guid TenantId { get; set; }

    public DateOnly TradeDate { get; set; }
    public string Exchange { get; set; } = string.Empty;        // NFO | BFO
    public string Segment { get; set; } = string.Empty;         // FO

    // Instrument
    public string InstrumentId { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string InstrumentName { get; set; } = string.Empty;
    public string ContractType { get; set; } = string.Empty;   // Index Future / Stock Future / Index Option / Stock Option
    public string Isin { get; set; } = string.Empty;
    public DateOnly? ExpiryDate { get; set; }
    public decimal StrikePrice { get; set; }
    public string OptionType { get; set; } = string.Empty;     // CE | PE | FX (futures)
    public long LotSize { get; set; }

    // OHLC
    public decimal OpenPrice { get; set; }
    public decimal HighPrice { get; set; }
    public decimal LowPrice { get; set; }
    public decimal ClosePrice { get; set; }
    public decimal LastTradedPrice { get; set; }
    public decimal PreviousClose { get; set; }
    public decimal UnderlyingPrice { get; set; }
    public decimal SettlementPrice { get; set; }

    // Open interest
    public long OpenInterest { get; set; }
    public long OpenInterestChange { get; set; }

    // Volume & turnover
    public long TotalVolume { get; set; }
    public decimal TotalTurnover { get; set; }
    public long TotalTrades { get; set; }

    public virtual FoFileImportBatch Batch { get; set; } = null!;
}
