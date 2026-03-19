namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.DTOs;

public record FoTradeBookItemDto(
    Guid Id,
    DateOnly TradeDate,
    string Segment,
    string Exchange,
    string UniqueTradeId,
    string ClearingMemberId,
    string BrokerId,
    string? BranchCode,
    string Symbol,
    string InstrumentName,
    string ContractType,          // FUTIDX | FUTSTK | OPTIDX | OPTSTK
    DateOnly? ExpiryDate,
    decimal StrikePrice,
    string OptionType,            // CE | PE | FX
    long LotSize,                 // FMULTIPLIER
    string ClientType,            // C = Client, P = Proprietary
    string ClientCode,
    string? CtclId,               // Exchange unique client terminal ID
    string? OriginalClientId,     // ORGCLENTID
    Guid? ClientId,
    string? ClientName,
    string? ClientStateCode,
    string Side,                  // B = Buy, S = Sell
    long Quantity,
    decimal NumberOfLots,
    decimal Price,
    decimal TradeValue,
    string SettlementType,
    string SettlementTransactionId,
    Guid BatchId
);

public record FoTradeBookPagedDto(
    IEnumerable<FoTradeBookItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);
