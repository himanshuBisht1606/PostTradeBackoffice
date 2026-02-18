using MediatR;
using PostTrade.Application.Features.MasterSetup.Tenants.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Tenants.Queries;

public record GetTenantByIdQuery(Guid TenantId) : IRequest<TenantDto?>;

public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, TenantDto?>
{
    private readonly IRepository<Tenant> _repo;

    public GetTenantByIdQueryHandler(IRepository<Tenant> repo) => _repo = repo;

    public async Task<TenantDto?> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var results = await _repo.FindAsync(t => t.TenantId == request.TenantId, cancellationToken);
        var t = results.FirstOrDefault();
        if (t is null) return null;
        return new TenantDto(t.TenantId, t.TenantCode, t.TenantName, t.ContactEmail, t.ContactPhone,
            t.Status, t.Address, t.City, t.Country, t.TaxId, t.LicenseExpiryDate);
    }
}
