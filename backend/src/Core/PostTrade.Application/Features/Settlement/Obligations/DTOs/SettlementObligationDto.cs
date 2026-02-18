using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.Settlement.Obligations.DTOs;

public record SettlementObligationDto(
    Guid ObligationId,
    Guid TenantId,
    Guid BrokerId,
    Guid ClientId,
    Guid BatchId,
    string SettlementNo,
    decimal FundsPayIn,
    decimal FundsPayOut,
    decimal NetFundsObligation,
    int SecuritiesPayIn,
    int SecuritiesPayOut,
    int NetSecuritiesObligation,
    ObligationStatus Status,
    DateTime? SettledAt
);
