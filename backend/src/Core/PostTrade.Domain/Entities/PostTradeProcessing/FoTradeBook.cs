namespace PostTrade.Domain.Entities.PostTradeProcessing;

/// <summary>
/// Main confirmed FO trade book — final ledger entry for every FO trade.
/// Populated from FoTradeDate (normalized staging) after validation.
///
/// TrnSlNo is copied from FoTradeDate for imported trades.
/// For BF / CF / EX / AS / CL / manual trades it is obtained from the
/// shared post_trade.fo_trn_slno_seq sequence at creation time.
/// </summary>
public class FoTradeBook : BaseEntity
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public Guid TenantId { get; set; }

    // ── Global unique serial ────────────────────────────────────────────────
    /// <summary>
    /// Transaction serial number — globally unique across all FO trade record types.
    /// Imported trades carry the TrnSlNo assigned when the FoTradeDate row was created.
    /// BF/CF/EX/AS/CL/manual trades get a new value from fo_trn_slno_seq at creation.
    /// </summary>
    public long TrnSlNo { get; set; }

    // ── Trade type ─────────────────────────────────────────────────────────
    /// <summary>
    /// Record type for this trade entry.
    /// "11" = Imported/Traded (from exchange file, equivalent of CFORise tradestatus='11').
    /// Other values: BF (Brought Forward), CF (Carried Forward),
    ///               EX (Exercise), AS (Assignment), CL (Closing).
    /// </summary>
    public string TrdType { get; set; } = "11";

    // ── Trade identity ─────────────────────────────────────────────────────
    public DateOnly TradeDate { get; set; }
    public string Segment { get; set; } = string.Empty;         // Exchange segment (FO)
    public string Exchange { get; set; } = string.Empty;        // Physical exchange: NFO | BFO
    /// <summary>
    /// Billing exchange — determined by clearing corporation from ExchangeSegment master.
    /// NCL (NSCCL) clearing → NFO; ICCL clearing → BFO.
    /// Both NFO and BFO trades cleared by NCL get GlobalExchange = "NFO" (one combined bill).
    /// </summary>
    public string GlobalExchange { get; set; } = string.Empty;
    public string UniqueTradeId { get; set; } = string.Empty;
    public string ClearingMemberId { get; set; } = string.Empty;
    public string BrokerId { get; set; } = string.Empty;
    public string? BranchCode { get; set; }

    // ── Instrument ─────────────────────────────────────────────────────────
    public string InstrumentId { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string InstrumentName { get; set; } = string.Empty;  // ContractName (e.g. FUTIDXNIFTY27MAR2025)
    public string ContractType { get; set; } = string.Empty;    // FUTIDX | FUTSTK | OPTIDX | OPTSTK
    public string UnderlyingSymbol { get; set; } = string.Empty;
    public string Isin { get; set; } = string.Empty;
    public DateOnly? ExpiryDate { get; set; }
    public decimal StrikePrice { get; set; }
    public string OptionType { get; set; } = string.Empty;      // CE | PE | FX
    public long LotSize { get; set; }
    /// <summary>Price multiplier (CMULTIPLIER) from FoContract — used for TradeValue = Qty × Price × FMultiplier.</summary>
    public decimal FMultiplier { get; set; } = 1m;

    // ── Client ─────────────────────────────────────────────────────────────
    public string ClientType { get; set; } = string.Empty;      // C = Client, P = Proprietary
    public string ClientCode { get; set; } = string.Empty;
    public string? CtclId { get; set; }
    public string? OriginalClientId { get; set; }               // OrgnlCtdnPtcptId (custodian participant)
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? ClientStateCode { get; set; }
    /// <summary>True when OrgnlCtdnPtcptId is populated — custodian/institutional trade (CP_FLAG=Y in CFORise).</summary>
    public bool IsCustodianTrade { get; set; }

    // ── Trade execution ────────────────────────────────────────────────────
    public string Side { get; set; } = string.Empty;            // B = Buy, S = Sell
    public DateTime? TradeDateTime { get; set; }
    public string? TradeStatus { get; set; }                    // OR | CN
    public string? OrderRef { get; set; }
    public string? MarketType { get; set; }
    public string? BookType { get; set; }
    public string? BookTypeName { get; set; }
    public long Quantity { get; set; }
    public decimal NumberOfLots { get; set; }                   // FO_UNIT = Qty / LotSize
    public decimal Price { get; set; }
    public decimal? NetPrice { get; set; }                      // Price pre-brokerage (= Price at import; adjusted later)
    public decimal? Brokerage { get; set; }
    public decimal TradeValue { get; set; }                     // Qty × Price × FMultiplier
    public decimal? ExerciseAssignmentPrice { get; set; }
    public string? CounterpartyCode { get; set; }
    public string? Remarks { get; set; }
    public string SettlementType { get; set; } = string.Empty;
    public string SettlementTransactionId { get; set; } = string.Empty;
    public DateOnly? SettlementDate { get; set; }

    public virtual FoFileImportBatch Batch { get; set; } = null!;
}
