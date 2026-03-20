namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.DTOs;

public record FoContractMasterDto(
    Guid ContractRowId,
    DateOnly TradingDate,
    string Exchange,
    string FinInstrmId,
    string TckrSymb,
    string FinInstrmNm,
    string XpryDt,
    DateOnly? ExpiryDate,
    decimal StrkPric,
    string OptnTp,
    string FinInstrmTp,
    string SttlmMtd,
    string StockNm,
    long MinLot,
    long NewBrdLotQty,
    Guid? RegisteredInstrumentId
);
