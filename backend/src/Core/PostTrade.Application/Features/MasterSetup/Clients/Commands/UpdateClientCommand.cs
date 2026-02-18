using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.Clients.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Clients.Commands;

public record UpdateClientCommand(
    Guid ClientId,
    string ClientName,
    string Email,
    string Phone,
    ClientStatus Status,
    string? PAN,
    string? Address,
    string? BankAccountNo,
    string? BankName,
    string? BankIFSC
) : IRequest<ClientDto?>;

public class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientCommandValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.ClientName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
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

        client.ClientName = request.ClientName;
        client.Email = request.Email;
        client.Phone = request.Phone;
        client.Status = request.Status;
        client.PAN = request.PAN;
        client.Address = request.Address;
        client.BankAccountNo = request.BankAccountNo;
        client.BankName = request.BankName;
        client.BankIFSC = request.BankIFSC;

        await _repo.UpdateAsync(client, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ClientDto(client.ClientId, client.TenantId, client.BrokerId, client.ClientCode,
            client.ClientName, client.Email, client.Phone, client.ClientType, client.Status,
            client.PAN, client.Address, client.BankAccountNo, client.BankName);
    }
}
