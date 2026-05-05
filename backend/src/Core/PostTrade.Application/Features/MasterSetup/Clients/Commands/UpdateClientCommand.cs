using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.Clients.DTOs;
using PostTrade.Application.Features.MasterSetup.Clients.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Clients.Commands;

public record UpdateClientCommand(
    Guid ClientId,
    Guid? BranchId,
    string ClientName,
    string Email,
    string Phone,
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
    // Extended personal
    string? Gender = null,
    string? DateOfBirth = null,
    string? MaritalStatus = null,
    string? Occupation = null,
    string? GrossAnnualIncome = null,
    string? FatherSpouseName = null,
    string? MotherName = null,
    // Extended contact & address
    string? AlternateMobile = null,
    string? City = null,
    string? PinCode = null,
    string? CorrespondenceAddress = null,
    // Extended bank
    string? AccountType = null,
    string? BranchName = null
) : IRequest<ClientDetailDto?>;

public class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientCommandValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.ClientName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Aadhaar).Length(12).When(x => x.Aadhaar != null);
        RuleFor(x => x.StateCode).MaximumLength(10).When(x => x.StateCode != null);
        RuleFor(x => x.AlternateMobile).MaximumLength(20).When(x => x.AlternateMobile != null);
        RuleFor(x => x.PinCode).MaximumLength(10).When(x => x.PinCode != null);
    }
}

public class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand, ClientDetailDto?>
{
    private readonly IRepository<Client> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateClientCommandHandler(
        IRepository<Client> repo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ClientDetailDto?> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(
            c => c.ClientId == request.ClientId && c.TenantId == tenantId,
            cancellationToken);
        var client = results.FirstOrDefault();
        if (client is null) return null;

        // Core fields
        client.BranchId = request.BranchId;
        client.ClientName = request.ClientName;
        client.Email = request.Email;
        client.Phone = request.Phone;
        client.Status = request.Status;
        client.PAN = request.PAN;
        client.Aadhaar = request.Aadhaar;
        client.DPId = request.DPId;
        client.DematAccountNo = request.DematAccountNo;
        client.Depository = request.Depository;
        client.Address = request.Address;
        client.StateCode = request.StateCode;
        client.StateName = request.StateName;
        client.BankAccountNo = request.BankAccountNo;
        client.BankName = request.BankName;
        client.BankIFSC = request.BankIFSC;
        client.KYCStatus = request.KYCStatus;
        client.RiskCategory = request.RiskCategory;

        // Extended personal (only update if provided — null means "leave as-is")
        if (request.Gender is not null) client.Gender = request.Gender;
        if (request.DateOfBirth is not null && DateOnly.TryParse(request.DateOfBirth, out var dob))
            client.DateOfBirth = dob;
        if (request.MaritalStatus is not null) client.MaritalStatus = request.MaritalStatus;
        if (request.Occupation is not null) client.Occupation = request.Occupation;
        if (request.GrossAnnualIncome is not null) client.GrossAnnualIncome = request.GrossAnnualIncome;
        if (request.FatherSpouseName is not null) client.FatherSpouseName = request.FatherSpouseName;
        if (request.MotherName is not null) client.MotherName = request.MotherName;

        // Extended contact & address
        if (request.AlternateMobile is not null) client.AlternateMobile = request.AlternateMobile;
        if (request.City is not null) client.City = request.City;
        if (request.PinCode is not null) client.PinCode = request.PinCode;
        if (request.CorrespondenceAddress is not null) client.CorrespondenceAddress = request.CorrespondenceAddress;

        // Extended bank
        if (request.AccountType is not null) client.AccountType = request.AccountType;
        if (request.BranchName is not null) client.BranchName = request.BranchName;

        await _repo.UpdateAsync(client, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GetClientByIdQueryHandler.Map(client);
    }
}
