using MediatR;
using PostTrade.Application.Features.MasterSetup.Brokers.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Brokers.Queries;

public record GetBrokerByIdQuery(Guid BrokerId) : IRequest<BrokerDto?>;

public class GetBrokerByIdQueryHandler : IRequestHandler<GetBrokerByIdQuery, BrokerDto?>
{
    private readonly IRepository<Broker> _repo;
    private readonly ITenantContext _tenantContext;

    public GetBrokerByIdQueryHandler(IRepository<Broker> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<BrokerDto?> Handle(GetBrokerByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(b => b.BrokerId == request.BrokerId && b.TenantId == tenantId, cancellationToken);
        var b = results.FirstOrDefault();
        if (b is null) return null;
        return new BrokerDto(b.BrokerId, b.TenantId, b.BrokerCode, b.BrokerName,
            b.SEBIRegistrationNo, b.ContactEmail, b.ContactPhone, b.Status, b.Address, b.PAN, b.GST);
    }
}
