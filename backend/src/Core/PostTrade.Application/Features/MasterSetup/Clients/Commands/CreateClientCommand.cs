using FluentValidation;
using MediatR;
using PostTrade.Application.Features.MasterSetup.Clients.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.MasterSetup.Clients.Commands;

public record CreateClientCommand(
    Guid BrokerId,
    string ClientCode,
    string ClientName,
    string Email,
    string Phone,
    ClientType ClientType,
    string? PAN,
    string? Address,
    string? BankAccountNo,
    string? BankName,
    string? BankIFSC
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
            ClientCode = request.ClientCode,
            ClientName = request.ClientName,
            Email = request.Email,
            Phone = request.Phone,
            ClientType = request.ClientType,
            PAN = request.PAN,
            Address = request.Address,
            BankAccountNo = request.BankAccountNo,
            BankName = request.BankName,
            BankIFSC = request.BankIFSC,
            Status = ClientStatus.Active
        };

        await _repo.AddAsync(client, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ClientDto(client.ClientId, client.TenantId, client.BrokerId, client.ClientCode,
            client.ClientName, client.Email, client.Phone, client.ClientType, client.Status,
            client.PAN, client.Address, client.BankAccountNo, client.BankName);
    }
}
