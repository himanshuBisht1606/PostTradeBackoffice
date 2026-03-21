namespace PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.DTOs;

public record CmScripMasterDto(
    Guid CmScripMasterId,
    string Exchange,
    DateOnly TradingDate,
    string Symbol,
    string ISIN,
    string Series,
    string Name,
    decimal FaceValue,
    int LotSize,
    decimal TickSize,
    string InstrumentType
);
