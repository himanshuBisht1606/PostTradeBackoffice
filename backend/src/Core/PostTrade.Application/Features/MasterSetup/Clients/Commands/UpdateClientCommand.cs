using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.Clients.DTOs;
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
    RiskCategory RiskCategory
) : IRequest<ClientDto?>;

public class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientCommandValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.ClientName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Aadhaar).Length(12).When(x => x.Aadhaar != null);
        RuleFor(x => x.StateCode).Length(2).When(x => x.StateCode != null);
    }
}

public class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand, ClientDto?>
{
    private readonly IRepository<Client> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateClientCommandHandler(IRepository<Client> repo, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ClientDto?> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(c => c.ClientId == request.ClientId && c.TenantId == tenantId, cancellationToken);
        var client = results.FirstOrDefault();
        if (client is null) return null;

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

        await _repo.UpdateAsync(client, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ClientDto(client.ClientId, client.TenantId, client.BrokerId, client.BranchId,
            client.ClientCode, client.ClientName, client.Email, client.Phone, client.ClientType, client.Status,
            client.PAN, client.Aadhaar, client.DPId, client.DematAccountNo, client.Depository,
            client.Address, client.StateCode, client.StateName, client.BankAccountNo, client.BankName,
            client.BankIFSC, client.KYCStatus, client.RiskCategory);
    }
}
