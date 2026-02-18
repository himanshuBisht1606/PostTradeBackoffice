using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Clients.DTOs;

public record ClientDto(
    Guid ClientId,
    Guid TenantId,
    Guid BrokerId,
    string ClientCode,
    string ClientName,
    string Email,
    string Phone,
    ClientType ClientType,
    ClientStatus Status,
    string? PAN,
    string? Address,
    string? BankAccountNo,
    string? BankName
);
