using MediatR;
using PostTrade.Application.Features.MasterSetup.Roles.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Roles.Queries;

public record GetRolesQuery : IRequest<IEnumerable<RoleDto>>;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, IEnumerable<RoleDto>>
{
    private readonly IRepository<Role> _repo;
    private readonly ITenantContext _tenantContext;

    public GetRolesQueryHandler(IRepository<Role> repo, ITenantContext tenantContext)
    {
        _repo = repo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var roles = await _repo.FindAsync(r => r.TenantId == tenantId, cancellationToken);
        return roles.Select(r => new RoleDto(r.RoleId, r.TenantId, r.RoleName, r.Description, r.IsSystemRole));
    }
}
