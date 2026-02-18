using MediatR;
using PostTrade.Application.Features.MasterSetup.Users.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Users.Queries;

public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto?>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IRepository<User> _repo;
    private readonly IRepository<UserRole> _userRoleRepo;
    private readonly IRepository<Role> _roleRepo;
    private readonly ITenantContext _tenantContext;

    public GetUserByIdQueryHandler(IRepository<User> repo, IRepository<UserRole> userRoleRepo,
        IRepository<Role> roleRepo, ITenantContext tenantContext)
    {
        _repo = repo;
        _userRoleRepo = userRoleRepo;
        _roleRepo = roleRepo;
        _tenantContext = tenantContext;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var results = await _repo.FindAsync(u => u.UserId == request.UserId && u.TenantId == tenantId, cancellationToken);
        var user = results.FirstOrDefault();
        if (user is null) return null;

        var userRoles = await _userRoleRepo.FindAsync(ur => ur.UserId == user.UserId, cancellationToken);
        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        var roles = roleIds.Count > 0
            ? await _roleRepo.FindAsync(r => roleIds.Contains(r.RoleId), cancellationToken)
            : Enumerable.Empty<Role>();

        return new UserDto(user.UserId, user.TenantId, user.Username, user.Email,
            user.FirstName, user.LastName, user.Phone, user.Status, user.LastLoginAt,
            roles.Select(r => r.RoleName));
    }
}
