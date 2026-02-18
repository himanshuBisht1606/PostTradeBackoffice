using MediatR;
using PostTrade.Application.Features.MasterSetup.Users.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Users.Queries;

public record GetUsersQuery : IRequest<IEnumerable<UserDto>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IEnumerable<UserDto>>
{
    private readonly IRepository<User> _repo;
    private readonly IRepository<UserRole> _userRoleRepo;
    private readonly IRepository<Role> _roleRepo;
    private readonly ITenantContext _tenantContext;

    public GetUsersQueryHandler(IRepository<User> repo, IRepository<UserRole> userRoleRepo,
        IRepository<Role> roleRepo, ITenantContext tenantContext)
    {
        _repo = repo;
        _userRoleRepo = userRoleRepo;
        _roleRepo = roleRepo;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var users = await _repo.FindAsync(u => u.TenantId == tenantId, cancellationToken);
        var userIds = users.Select(u => u.UserId).ToList();

        var allUserRoles = await _userRoleRepo.FindAsync(ur => userIds.Contains(ur.UserId), cancellationToken);
        var roleIds = allUserRoles.Select(ur => ur.RoleId).Distinct().ToList();
        var roles = roleIds.Count > 0
            ? await _roleRepo.FindAsync(r => roleIds.Contains(r.RoleId), cancellationToken)
            : Enumerable.Empty<Role>();

        var roleMap = roles.ToDictionary(r => r.RoleId, r => r.RoleName);
        var userRoleMap = allUserRoles.GroupBy(ur => ur.UserId)
            .ToDictionary(g => g.Key, g => g.Select(ur => roleMap.GetValueOrDefault(ur.RoleId, "")).ToList());

        return users.Select(u => new UserDto(u.UserId, u.TenantId, u.Username, u.Email,
            u.FirstName, u.LastName, u.Phone, u.Status, u.LastLoginAt,
            userRoleMap.GetValueOrDefault(u.UserId, [])));
    }
}
