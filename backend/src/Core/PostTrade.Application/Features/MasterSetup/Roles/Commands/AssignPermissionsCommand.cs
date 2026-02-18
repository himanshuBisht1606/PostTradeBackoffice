using MediatR;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Application.Features.MasterSetup.Roles.Commands;

public record AssignPermissionsCommand(Guid RoleId, IEnumerable<Guid> PermissionIds) : IRequest<bool>;

public class AssignPermissionsCommandHandler : IRequestHandler<AssignPermissionsCommand, bool>
{
    private readonly IRepository<Role> _roleRepo;
    private readonly IRepository<RolePermission> _rpRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public AssignPermissionsCommandHandler(IRepository<Role> roleRepo, IRepository<RolePermission> rpRepo,
        IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _roleRepo = roleRepo;
        _rpRepo = rpRepo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<bool> Handle(AssignPermissionsCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        var roles = await _roleRepo.FindAsync(r => r.RoleId == request.RoleId && r.TenantId == tenantId, cancellationToken);
        if (!roles.Any()) return false;

        var existing = await _rpRepo.FindAsync(rp => rp.RoleId == request.RoleId, cancellationToken);
        foreach (var rp in existing)
            await _rpRepo.DeleteAsync(rp, cancellationToken);

        foreach (var permId in request.PermissionIds)
        {
            await _rpRepo.AddAsync(new RolePermission
            {
                RolePermissionId = Guid.NewGuid(),
                RoleId = request.RoleId,
                PermissionId = permId
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
