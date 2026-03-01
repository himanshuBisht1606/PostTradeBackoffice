using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Clients.DTOs;

/// <summary>
/// Extended DTO returned by GET /api/clients/{id} — includes all editable onboarding fields.
/// The lighter ClientDto is used for list responses.
/// </summary>
public record ClientDetailDto(
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
    RiskCategory RiskCategory,
    // Extended personal fields
    string? DateOfBirth,
    string? Gender,
    string? MaritalStatus,
    string? Occupation,
    string? GrossAnnualIncome,
    string? FatherSpouseName,
    string? MotherName,
    // Extended contact & address
    string? AlternateMobile,
    string? City,
    string? PinCode,
    string? CorrespondenceAddress,
    // Extended identity
    string HolderType,
    string? CitizenshipStatus,
    string? ResidentialStatus,
    // Extended bank & demat
    string? AccountType,
    string? BranchName
);
