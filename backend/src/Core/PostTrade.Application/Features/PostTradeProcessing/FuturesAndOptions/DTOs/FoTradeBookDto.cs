namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.DTOs;

public record FoTradeBookItemDto(
    Guid Id,
    DateOnly TradeDate,
    DateTime? TradeDateTime,        // Full trade timestamp (TRADE_TIME)
    string Segment,
    string Exchange,
    string UniqueTradeId,           // Exchange trade number (TRADENO)
    string? TradeStatus,            // OR=Open, CN=Cancelled (TRADESTATUS)
    string? OrderRef,               // Exchange order reference (ORDERNO)
    string ClearingMemberId,        // CMID
    string BrokerId,                // TMID
    string? BranchCode,             // BRANCHID
    string Symbol,
    string InstrumentName,          // SECURITYNAME
    string ContractType,            // FUTIDX | FUTSTK | OPTIDX | OPTSTK (INSTRUMENT_TYPE)
    DateOnly? ExpiryDate,
    decimal StrikePrice,
    string OptionType,              // CE | PE | FX (OPTIONTYPE)
    long LotSize,                   // FMULTIPLIER / LOT
    string ClientType,              // C = Client, P = Proprietary (BOOKTYPE)
    string ClientCode,              // CLIENTCODE
    string? CtclId,                 // TRN_CTCLID
    string? OriginalClientId,       // ORGCLIENTID
    Guid? ClientId,
    string? ClientName,
    string? ClientStateCode,        // CLIENT_STATECD
    string Side,                    // B = Buy, S = Sell (BUYSELLIND)
    string? MarketType,             // MKT_TYPE
    long Quantity,                  // TRN_QTY
    decimal NumberOfLots,
    decimal Price,                  // TRADEPRICE
    decimal? NetPrice,              // NETPRICE
    decimal? Brokerage,             // TRN_BROK
    decimal TradeValue,
    string? Remarks,                // REMARK
    string SettlementType,
    string SettlementTransactionId,
    DateOnly? SettlementDate,       // SETTLEMENT_DATE
    Guid BatchId
);

public record FoTradeBookPagedDto(
    IEnumerable<FoTradeBookItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);
