using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Brokers.DTOs;

public record BrokerExchangeMembershipDto(
    Guid BrokerExchangeMembershipId,
    Guid BrokerId,
    Guid ExchangeSegmentId,
    string ExchangeSegmentCode,
    string ExchangeSegmentName,
    string TradingMemberId,
    string? ClearingMemberId,
    MembershipType MembershipType,
    DateOnly EffectiveDate,
    DateOnly? ExpiryDate,
    bool IsActive
);
