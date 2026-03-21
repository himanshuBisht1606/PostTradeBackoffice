namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.DTOs;

/// <summary>
/// Raw exchange fields — used internally and for admin views.
/// </summary>
public record FoContractMasterDto(
    Guid ContractRowId,
    DateOnly TradingDate,
    string Exchange,
    string FinInstrmId,
    string UndrlygFinInstrmId,
    string TckrSymb,
    string FinInstrmNm,          // Full contract name (e.g. BANKNIFTY26MAR74300CE)
    string XpryDt,
    DateOnly? ExpiryDate,
    decimal StrkPric,
    string OptnTp,               // CE | PE | blank (futures)
    string FinInstrmTp,          // FUTIDX | FUTSTK | OPTIDX | OPTSTK
    string? OptnExrcStyle,       // E=European | A=American
    string SttlmMtd,
    long MinLot,
    long NewBrdLotQty,
    decimal TickSize,            // Bid interval / tick size
    decimal? BasePric,           // Base/reference price
    string? MktTpAndId,          // Market type
    string? Isin,
    Guid? RegisteredInstrumentId
);

/// <summary>
/// Broker-facing contract book view — matches the cONTRACT.xls reference format.
/// Columns: INSTRUMENT_TYPE, SYMBOL, LOTSIZE, EXPIRYDATE, CONTNAME, EXCHANGE,
///          CMULTIPLIER, STRIKE, OPTTYPE, INTEROPSYMBOL, SEGMENT, OLD_EXPIRYDATE
/// </summary>
public record FoContractBookItemDto(
    string InstrumentType,      // FUTIDX | FUTSTK | OPTIDX | OPTSTK
    string Symbol,              // Ticker symbol (e.g. NIFTY, RELIANCE)
    long LotSize,               // Contract lot size
    DateOnly? ExpiryDate,       // Contract expiry date
    string ContName,            // Broker contract name: InstrType+Symbol+DDMmmYYYY
    string Exchange,            // NFO | BFO
    int CMultiplier,            // Contract multiplier (1 for FO equity)
    decimal Strike,             // Strike price in ₹ (paise ÷ 100)
    string OptionType,          // CE | PE | blank (futures)
    string? InteropSymbol,      // Interoperability / cross-exchange symbol
    decimal TickSize,           // Tick / bid interval
    string? OptionExerciseStyle, // E=European | A=American
    string Segment,             // FO
    string? Isin,
    Guid ContractRowId
);

/// <summary>
/// Curated FO contract — from FoContracts table (post-filtered, normalized fields).
/// Strike is already in ₹ (paise ÷ 100). InstrumentType is normalized (FUTIDX/FUTSTK/OPTIDX/OPTSTK).
/// Used by Master Setup → FO Instruments view.
/// </summary>
public record FoContractDto(
    Guid ContractId,
    string Exchange,
    DateOnly TradingDate,
    string InstrumentType,      // FUTIDX | FUTSTK | OPTIDX | OPTSTK
    string Symbol,
    string ContractName,        // FUTIDXNIFTY27MAR2025 — display name
    DateOnly ExpiryDate,
    decimal StrikePrice,        // in ₹ (paise ÷ 100)
    string OptionType,          // CE | PE | FX
    long LotSize,
    decimal FMultiplier,
    string? FinInstrmId,
    string UnderlyingSymbol,
    string? Isin,
    decimal TickSize,
    string? SttlmMtd,
    Guid? RegisteredInstrumentId
);

public record FoContractBookPagedDto(
    IEnumerable<FoContractBookItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);
