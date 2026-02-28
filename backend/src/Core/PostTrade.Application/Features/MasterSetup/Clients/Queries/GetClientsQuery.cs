using MediatR;
using PostTrade.Application.Features.MasterSetup.Clients.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Clients.Queries;

public record GetClientsQuery(int Page = 1, int PageSize = 20) : IRequest<IEnumerable<ClientDto>>;

public class GetClientsQueryHandler : IRequestHandler<GetClientsQuery, IEnumerable<ClientDto>>
{
    private readonly IRepository<Client> _repo;
    private readonly ITenantContext _tenantContext;

    public GetClientsQueryHandler(IRepository<Client> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<ClientDto>> Handle(GetClientsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var clients = await _repo.FindAsync(c => c.TenantId == tenantId, cancellationToken);
        return clients
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new ClientDto(c.ClientId, c.TenantId, c.BrokerId, c.BranchId,
                c.ClientCode, c.ClientName, c.Email, c.Phone, c.ClientType, c.Status,
                c.PAN, c.Aadhaar, c.DPId, c.DematAccountNo, c.Depository,
                c.Address, c.StateCode, c.StateName, c.BankAccountNo, c.BankName,
                c.BankIFSC, c.KYCStatus, c.RiskCategory));
    }
}
