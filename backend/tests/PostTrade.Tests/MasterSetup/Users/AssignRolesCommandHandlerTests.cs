using PostTrade.Application.Features.MasterSetup.Users.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Tests.MasterSetup.Users;

public class AssignRolesCommandHandlerTests
{
    private readonly Mock<IRepository<User>>     _userRepo      = new();
    private readonly Mock<IRepository<UserRole>> _userRoleRepo  = new();
    private readonly Mock<IUnitOfWork>           _unitOfWork    = new();
    private readonly Mock<ITenantContext>        _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private AssignRolesCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _userRoleRepo.Setup(r => r.AddAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((UserRole ur, CancellationToken _) => ur);
        _userRoleRepo.Setup(r => r.DeleteAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);
        return new AssignRolesCommandHandler(_userRepo.Object, _userRoleRepo.Object, _unitOfWork.Object, _tenantContext.Object);
    }

    private User ExistingUser(Guid userId) => new()
    {
        UserId   = userId,
        TenantId = TenantId,
        Username = "jdoe"
    };

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFalse()
    {
        _userRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<User>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new AssignRolesCommand(Guid.NewGuid(), [Guid.NewGuid()]), CancellationToken.None);

        result.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldReturnTrue()
    {
        var userId = Guid.NewGuid();

        _userRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<User> { ExistingUser(userId) });
        _userRoleRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<UserRole>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new AssignRolesCommand(userId, [Guid.NewGuid()]), CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldDeleteExistingRolesBeforeAddingNew()
    {
        var userId      = Guid.NewGuid();
        var existingRole = new UserRole { UserRoleId = Guid.NewGuid(), UserId = userId, RoleId = Guid.NewGuid() };

        _userRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<User> { ExistingUser(userId) });
        _userRoleRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<UserRole> { existingRole });

        var handler = CreateHandler();
        await handler.Handle(new AssignRolesCommand(userId, [Guid.NewGuid()]), CancellationToken.None);

        _userRoleRepo.Verify(r => r.DeleteAsync(existingRole, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldAddOneUserRolePerRoleId()
    {
        var userId  = Guid.NewGuid();
        var roleIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        _userRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<User> { ExistingUser(userId) });
        _userRoleRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<UserRole>());

        var handler = CreateHandler();
        await handler.Handle(new AssignRolesCommand(userId, roleIds), CancellationToken.None);

        _userRoleRepo.Verify(r => r.AddAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges()
    {
        var userId = Guid.NewGuid();

        _userRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<User> { ExistingUser(userId) });
        _userRoleRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<UserRole>());

        var handler = CreateHandler();
        await handler.Handle(new AssignRolesCommand(userId, [Guid.NewGuid()]), CancellationToken.None);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
