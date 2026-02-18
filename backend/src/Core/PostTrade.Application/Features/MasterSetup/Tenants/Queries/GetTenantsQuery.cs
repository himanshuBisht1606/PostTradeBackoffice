using MediatR;
using PostTrade.Application.Features.MasterSetup.Tenants.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Tenants.Queries;

public record GetTenantsQuery : IRequest<IEnumerable<TenantDto>>;

public class GetTenantsQueryHandler : IRequestHandler<GetTenantsQuery, IEnumerable<TenantDto>>
{
    private readonly IRepository<Tenant> _repo;

    public GetTenantsQueryHandler(IRepository<Tenant> repo) => _repo = repo;

    public async Task<IEnumerable<TenantDto>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenants = await _repo.GetAllAsync(cancellationToken);
        return tenants.Select(t => new TenantDto(
            t.TenantId, t.TenantCode, t.TenantName, t.ContactEmail, t.ContactPhone,
            t.Status, t.Address, t.City, t.Country, t.TaxId, t.LicenseExpiryDate));
    }
}
