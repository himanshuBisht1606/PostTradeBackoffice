namespace PostTrade.Domain.Entities.PostTradeProcessing;

/// <summary>
/// Date-level normalized trade staging table — equivalent of CFORise FOTRNMAST_YYYYMMDD.
/// Populated from FoTrade (raw staging) after:
///   • Filtering cancelled trades (RptdTxSts = 'CN')
///   • Deduplicating by UniqueTradeId within the trading date
///   • Enriching with LotSize, FMultiplier, ContractName from FoContract
/// Promoted to FoTradeBook (main confirmed trade book) after validation.
///
/// TrnSlNo is the global trade serial number generated from the shared
/// post_trade.fo_trn_slno_seq PostgreSQL sequence (equivalent of SYSDBSEQUENCE.NEXTVAL).
/// The same sequence is used for all trade record types:
///   Imported | BF | CF | EX | AS | CL | ManualTrade
/// </summary>
public class FoTradeDate : BaseEntity
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public Guid TenantId { get; set; }

    // ── Global unique serial (generated from fo_trn_slno_seq on insert) ───
    public long TrnSlNo { get; set; }

    // ── Trade identity ─────────────────────────────────────────────────────
    public DateOnly TradeDate { get; set; }
    public string Exchange { get; set; } = string.Empty;           // Physical exchange: NFO | BFO
    public string GlobalExchange { get; set; } = string.Empty;     // Billing exchange from ExchangeSegment.GlobalExchangeCode (NCL→NFO, ICCL→BFO)
    public string Segment { get; set; } = string.Empty;            // FO
    public string UniqueTradeId { get; set; } = string.Empty;      // UnqTradIdr (exchange unique)

    /// <summary>
    /// Trade record type — "11" for imported/traded (equivalent of CFORise tradestatus='11').
    /// Later values: BF (Brought Forward), CF (Carried Forward), EX (Exercise), AS (Assignment), CL (Closing).
    /// </summary>
    public string TrdType { get; set; } = "11";

    // ── Members ────────────────────────────────────────────────────────────
    public string ClearingMemberId { get; set; } = string.Empty;   // ClrMmbId
    public string TradingMemberId { get; set; } = string.Empty;    // Brkr / TMID

    // ── Instrument ─────────────────────────────────────────────────────────
    public string InstrumentType { get; set; } = string.Empty;     // FUTIDX | FUTSTK | OPTIDX | OPTSTK
    public string Symbol { get; set; } = string.Empty;             // TckrSymb
    public string ContractName { get; set; } = string.Empty;       // UPPER(InstrType+Symbol+ExpiryDate_DDMONYYYY)
    public string InstrumentId { get; set; } = string.Empty;       // FinInstrmId (exchange token)
    public DateOnly? ExpiryDate { get; set; }
    public decimal StrikePrice { get; set; }
    public string OptionType { get; set; } = string.Empty;         // CE | PE | FX
    public string UnderlyingSymbol { get; set; } = string.Empty;
    public string? Isin { get; set; }

    // ── Lot / multiplier enrichment (from FoContract) ──────────────────────
    public long LotSize { get; set; }                              // MinLot from FoContract
    public decimal FMultiplier { get; set; } = 1m;                 // CMULTIPLIER from FoContract

    // ── Client ─────────────────────────────────────────────────────────────
    public string ClientCode { get; set; } = string.Empty;         // ClntId (exchange client code)
    public string OriginalClientCode { get; set; } = string.Empty; // OrgnlCtdnPtcptId (ORGCLIENTID)
    public string ClientType { get; set; } = string.Empty;         // C=Client, P=Proprietary
    public string? CtclId { get; set; }
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? ClientStateCode { get; set; }
    public bool IsCustodianTrade { get; set; }                     // true if OrgnlCtdnPtcptId non-blank (CP_FLAG=Y)

    // ── Trade execution ────────────────────────────────────────────────────
    public string Side { get; set; } = string.Empty;               // B | S (BuySellInd)
    public long Quantity { get; set; }                             // TradQty (always positive)
    public decimal NumberOfLots { get; set; }                      // FO_UNIT = Qty / LotSize
    public decimal Price { get; set; }
    public decimal NetPrice { get; set; }                          // = Price (pre-brokerage default)
    public decimal TradeValue { get; set; }                        // Qty * Price * FMultiplier
    public DateTime? TradeDateTime { get; set; }                   // TradDtTm (UTC)
    public string? TradeStatus { get; set; }                       // OR (CN trades excluded)
    public string? OrderRef { get; set; }                          // OrdrRef
    public string? MarketType { get; set; }                        // MktTpandId
    public string BookType { get; set; } = "RL";                   // Regular Lot
    public string BookTypeName { get; set; } = "RL";

    // ── Settlement ─────────────────────────────────────────────────────────
    public string? SettlementType { get; set; }
    public string? SettlementTransactionId { get; set; }
    public DateOnly? SettlementDate { get; set; }

    // ── Misc ───────────────────────────────────────────────────────────────
    public string? CounterpartyCode { get; set; }
    public string? Remarks { get; set; }
    public string? FileName { get; set; }

    public virtual FoFileImportBatch Batch { get; set; } = null!;
}
