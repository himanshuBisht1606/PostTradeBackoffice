using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Brokers.DTOs;

/// <summary>List view — only identity + status fields.</summary>
public record BrokerDto(
    Guid BrokerId,
    Guid TenantId,
    string BrokerCode,
    string BrokerName,
    BrokerEntityType EntityType,
    BrokerStatus Status,
    string? SEBIRegistrationNo,
    string ContactEmail,
    string ContactPhone,
    string? RegisteredCity,
    string? RegisteredState,
    string? PAN,
    string? GST
);
