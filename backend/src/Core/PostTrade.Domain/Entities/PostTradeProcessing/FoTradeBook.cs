namespace PostTrade.Domain.Entities.PostTradeProcessing;

/// <summary>
/// Structured FO trade ledger — populated from FoTrades (staging) during import.
/// Uses descriptive column names instead of raw exchange-file abbreviations.
/// </summary>
public class FoTradeBook : BaseEntity
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public Guid TenantId { get; set; }

    // Trade identity
    public DateOnly TradeDate { get; set; }
    public string Segment { get; set; } = string.Empty;         // Exchange segment (FO)
    public string Exchange { get; set; } = string.Empty;        // NFO | BFO
    public string UniqueTradeId { get; set; } = string.Empty;
    public string ClearingMemberId { get; set; } = string.Empty; // Clearing member (NSCCL/ICCL)
    public string BrokerId { get; set; } = string.Empty;        // Trading member code
    public string? BranchCode { get; set; }                     // Trading member branch (BRANCHID)

    // Instrument
    public string InstrumentId { get; set; } = string.Empty;   // Exchange instrument identifier
    public string Symbol { get; set; } = string.Empty;         // Ticker symbol (e.g. NIFTY, RELIANCE)
    public string InstrumentName { get; set; } = string.Empty; // Full contract name (e.g. NIFTY26MARFUT)
    public string ContractType { get; set; } = string.Empty;   // Index Future / Stock Future / Index Option / Stock Option
    public string UnderlyingSymbol { get; set; } = string.Empty;
    public string Isin { get; set; } = string.Empty;
    public DateOnly? ExpiryDate { get; set; }
    public decimal StrikePrice { get; set; }
    public string OptionType { get; set; } = string.Empty;     // CE | PE | FX (futures)
    public long LotSize { get; set; }

    // Client
    public string ClientType { get; set; } = string.Empty;     // C = Client, P = Proprietary
    public string ClientCode { get; set; } = string.Empty;
    public string? CtclId { get; set; }                         // Exchange-assigned unique client terminal ID
    public string? OriginalClientId { get; set; }               // ORGCLENTID — original custodian participant ID
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? ClientStateCode { get; set; }                // Client GST/dealing state code

    // Trade execution
    public string Side { get; set; } = string.Empty;           // B = Buy, S = Sell
    public string? MarketType { get; set; }                     // Normal / Odd Lot / Auction (MKT_TYPE)
    public string? BookType { get; set; }                       // Book type code (BOOKTYPE)
    public string? BookTypeName { get; set; }                   // Book type name (BOOKTYPENAME)
    public long Quantity { get; set; }
    public decimal NumberOfLots { get; set; }
    public decimal Price { get; set; }
    public decimal? NetPrice { get; set; }                      // Price after brokerage adjustment (NETPRICE)
    public decimal? Brokerage { get; set; }                     // Broker commission (TRN_BROK)
    public decimal TradeValue { get; set; }
    public decimal? ExerciseAssignmentPrice { get; set; }       // Exercise/assignment price (EXAS_PRICE)
    public string? CounterpartyCode { get; set; }               // Counterparty/contra code (CONTRA_CODE)
    public string SettlementType { get; set; } = string.Empty;
    public string SettlementTransactionId { get; set; } = string.Empty;
    public DateOnly? SettlementDate { get; set; }               // Actual settlement date (SETTLEMENT_DATE)

    public virtual FoFileImportBatch Batch { get; set; } = null!;
}
