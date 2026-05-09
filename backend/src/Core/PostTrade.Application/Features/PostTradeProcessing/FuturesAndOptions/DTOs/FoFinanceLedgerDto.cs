namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.DTOs;

public record FoFinanceLedgerDto(
    Guid Id,
    Guid TenantId,
    DateOnly TradeDate,
    string Exchange,
    string ClearingMemberId,
    string BrokerId,
    string ClientCode,
    Guid? ClientId,
    string? ClientName,
    decimal BuyTurnover,
    decimal SellTurnover,
    decimal TotalTurnover,
    decimal TotalStt,
    decimal TotalStampDuty,
    decimal Brokerage,
    decimal ExchangeTransactionCharges,
    decimal SebiCharges,
    decimal Ipft,
    decimal GstOnCharges,
    decimal TotalCharges,
    decimal DailyMtmSettlement,
    decimal NetPremium,
    decimal FinalSettlement,
    decimal ExerciseAssignmentValue,
    decimal NetAmount
);
