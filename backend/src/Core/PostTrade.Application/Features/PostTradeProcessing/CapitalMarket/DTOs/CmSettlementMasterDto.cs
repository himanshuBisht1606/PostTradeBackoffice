namespace PostTrade.Application.Features.PostTradeProcessing.CapitalMarket.DTOs;

public record CmSettlementMasterDto(
    Guid CmSettlementMasterId,
    string Exchange,
    DateOnly TradingDate,
    string SettlementNo,
    string SettlementType,
    DateOnly PayInDate,
    DateOnly PayOutDate
);
