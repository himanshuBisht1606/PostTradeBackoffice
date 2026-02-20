using PostTrade.Application.Features.MasterSetup.Roles.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Tests.MasterSetup.Roles;

public class AssignPermissionsCommandHandlerTests
{
    private readonly Mock<IRepository<Role>>           _roleRepo      = new();
    private readonly Mock<IRepository<RolePermission>> _rpRepo        = new();
    private readonly Mock<IUnitOfWork>                 _unitOfWork    = new();
    private readonly Mock<ITenantContext>              _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private AssignPermissionsCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _rpRepo.Setup(r => r.AddAsync(It.IsAny<RolePermission>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((RolePermission rp, CancellationToken _) => rp);
        _rpRepo.Setup(r => r.DeleteAsync(It.IsAny<RolePermission>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);
        return new AssignPermissionsCommandHandler(_roleRepo.Object, _rpRepo.Object, _unitOfWork.Object, _tenantContext.Object);
    }

    private Role ExistingRole(Guid roleId) => new()
    {
        RoleId   = roleId,
        TenantId = TenantId,
        RoleName = "Analyst"
    };

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldReturnFalse()
    {
        _roleRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Role>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new AssignPermissionsCommand(Guid.NewGuid(), [Guid.NewGuid()]), CancellationToken.None);

        result.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoleExists_ShouldReturnTrue()
    {
        var roleId = Guid.NewGuid();

        _roleRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Role> { ExistingRole(roleId) });
        _rpRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<RolePermission, bool>>>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new List<RolePermission>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new AssignPermissionsCommand(roleId, [Guid.NewGuid()]), CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldDeleteExistingPermissionsBeforeAddingNew()
    {
        var roleId          = Guid.NewGuid();
        var existingPermission = new RolePermission { RolePermissionId = Guid.NewGuid(), RoleId = roleId, PermissionId = Guid.NewGuid() };

        _roleRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Role> { ExistingRole(roleId) });
        _rpRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<RolePermission, bool>>>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new List<RolePermission> { existingPermission });

        var handler = CreateHandler();
        await handler.Handle(new AssignPermissionsCommand(roleId, [Guid.NewGuid()]), CancellationToken.None);

        _rpRepo.Verify(r => r.DeleteAsync(existingPermission, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldAddOneRolePermissionPerPermissionId()
    {
        var roleId        = Guid.NewGuid();
        var permissionIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        _roleRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Role> { ExistingRole(roleId) });
        _rpRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<RolePermission, bool>>>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new List<RolePermission>());

        var handler = CreateHandler();
        await handler.Handle(new AssignPermissionsCommand(roleId, permissionIds), CancellationToken.None);

        _rpRepo.Verify(r => r.AddAsync(It.IsAny<RolePermission>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges()
    {
        var roleId = Guid.NewGuid();

        _roleRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Role> { ExistingRole(roleId) });
        _rpRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<RolePermission, bool>>>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new List<RolePermission>());

        var handler = CreateHandler();
        await handler.Handle(new AssignPermissionsCommand(roleId, [Guid.NewGuid()]), CancellationToken.None);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
