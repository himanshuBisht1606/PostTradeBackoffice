using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Brokers.DTOs;

public record BrokerDto(
    Guid BrokerId,
    Guid TenantId,
    string BrokerCode,
    string BrokerName,
    string? SEBIRegistrationNo,
    string ContactEmail,
    string ContactPhone,
    BrokerStatus Status,
    string? Address,
    string? PAN,
    string? GST
);
