using MediatR;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Users.Commands;

public record AssignRolesCommand(Guid UserId, IEnumerable<Guid> RoleIds) : IRequest<bool>;

public class AssignRolesCommandHandler : IRequestHandler<AssignRolesCommand, bool>
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<UserRole> _userRoleRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public AssignRolesCommandHandler(IRepository<User> userRepo, IRepository<UserRole> userRoleRepo,
        IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _userRepo = userRepo;
        _userRoleRepo = userRoleRepo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<bool> Handle(AssignRolesCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var users = await _userRepo.FindAsync(u => u.UserId == request.UserId && u.TenantId == tenantId, cancellationToken);
        if (!users.Any()) return false;

        // Remove existing roles
        var existing = await _userRoleRepo.FindAsync(ur => ur.UserId == request.UserId, cancellationToken);
        foreach (var ur in existing)
            await _userRoleRepo.DeleteAsync(ur, cancellationToken);

        // Add new roles
        foreach (var roleId in request.RoleIds)
        {
            await _userRoleRepo.AddAsync(new UserRole
            {
                UserRoleId = Guid.NewGuid(),
                UserId = request.UserId,
                RoleId = roleId,
                AssignedDate = DateTime.UtcNow
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
