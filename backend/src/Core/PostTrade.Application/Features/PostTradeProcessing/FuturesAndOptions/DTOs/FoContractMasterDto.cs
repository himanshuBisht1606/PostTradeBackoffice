namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.DTOs;

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
