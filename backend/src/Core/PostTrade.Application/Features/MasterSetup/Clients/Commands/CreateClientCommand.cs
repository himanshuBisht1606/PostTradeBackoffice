using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.Clients.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Clients.Commands;

public record CreateClientCommand(
    Guid BrokerId,
    Guid? BranchId,
    string ClientCode,
    string ClientName,
    string Email,
    string Phone,
    ClientType ClientType,
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
    KYCStatus KYCStatus = KYCStatus.Pending,
    RiskCategory RiskCategory = RiskCategory.Moderate
) : IRequest<ClientDto>;

public class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.BrokerId).NotEmpty();
        RuleFor(x => x.ClientCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ClientName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Aadhaar).Length(12).When(x => x.Aadhaar != null);
        RuleFor(x => x.StateCode).Length(2).When(x => x.StateCode != null);
    }
}

public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, ClientDto>
{
    private readonly IRepository<Client> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateClientCommandHandler(IRepository<Client> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ClientDto> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var client = new Client
        {
            ClientId = Guid.NewGuid(),
            TenantId = tenantId,
            BrokerId = request.BrokerId,
            BranchId = request.BranchId,
            ClientCode = request.ClientCode,
            ClientName = request.ClientName,
            Email = request.Email,
            Phone = request.Phone,
            ClientType = request.ClientType,
            PAN = request.PAN,
            Aadhaar = request.Aadhaar,
            DPId = request.DPId,
            DematAccountNo = request.DematAccountNo,
            Depository = request.Depository,
            Address = request.Address,
            StateCode = request.StateCode,
            StateName = request.StateName,
            BankAccountNo = request.BankAccountNo,
            BankName = request.BankName,
            BankIFSC = request.BankIFSC,
            KYCStatus = request.KYCStatus,
            RiskCategory = request.RiskCategory,
            Status = ClientStatus.Active
        };

        await _repo.AddAsync(client, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(client);
    }

    private static ClientDto MapToDto(Client c) => new(
        c.ClientId, c.TenantId, c.BrokerId, c.BranchId, c.ClientCode, c.ClientName,
        c.Email, c.Phone, c.ClientType, c.Status, c.PAN, c.Aadhaar, c.DPId,
        c.DematAccountNo, c.Depository, c.Address, c.StateCode, c.StateName,
        c.BankAccountNo, c.BankName, c.BankIFSC, c.KYCStatus, c.RiskCategory);
}
