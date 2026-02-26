using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Clients.DTOs;

public record ClientDto(
    Guid ClientId,
    Guid TenantId,
    Guid BrokerId,
    Guid? BranchId,
    string ClientCode,
    string ClientName,
    string Email,
    string Phone,
    ClientType ClientType,
    ClientStatus Status,
    string? PAN,
    string? Aadhaar,
    string? DPId,
    string? DematAccountNo,
    Depository? Depository,
    string? Address,
    string? StateCode,
    string? StateName,
    string? BankAccountNo,
    string? BankName,
    string? BankIFSC,
    KYCStatus KYCStatus,
    RiskCategory RiskCategory
);
