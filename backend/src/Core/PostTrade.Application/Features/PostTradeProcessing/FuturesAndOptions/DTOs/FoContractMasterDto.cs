namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.DTOs;

public record FoContractMasterDto(
    Guid ContractRowId,
    DateOnly TradingDate,
    string Exchange,
    string FinInstrmId,
    string TckrSymb,
    string FinInstrmNm,
    string XpryDt,
    decimal StrkPric,
    string OptnTp,
    string FinInstrmTp,
    string StockNm,
    long NewBrdLotQty
);
