using MediatR;
using PostTrade.Application.Features.MasterSetup.Brokers.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Brokers.Queries;

public record GetBrokersQuery : IRequest<IEnumerable<BrokerDto>>;

public class GetBrokersQueryHandler : IRequestHandler<GetBrokersQuery, IEnumerable<BrokerDto>>
{
    private readonly IRepository<Broker> _repo;
    private readonly ITenantContext _tenantContext;

    public GetBrokersQueryHandler(IRepository<Broker> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<BrokerDto>> Handle(GetBrokersQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var brokers = await _repo.FindAsync(b => b.TenantId == tenantId, cancellationToken);
        return brokers.Select(b => new BrokerDto(b.BrokerId, b.TenantId, b.BrokerCode, b.BrokerName,
            b.SEBIRegistrationNo, b.ContactEmail, b.ContactPhone, b.Status, b.Address, b.PAN, b.GST));
    }
}
