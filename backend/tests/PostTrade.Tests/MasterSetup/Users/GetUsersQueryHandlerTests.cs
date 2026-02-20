using PostTrade.Application.Features.MasterSetup.Users.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.MasterSetup.Users;

public class GetUsersQueryHandlerTests
{
    private readonly Mock<IRepository<User>>     _userRepo      = new();
    private readonly Mock<IRepository<UserRole>> _userRoleRepo  = new();
    private readonly Mock<IRepository<Role>>     _roleRepo      = new();
    private readonly Mock<ITenantContext>        _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private GetUsersQueryHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        return new GetUsersQueryHandler(_userRepo.Object, _userRoleRepo.Object, _roleRepo.Object, _tenantContext.Object);
    }

    private User MakeUser(int index = 1) => new()
    {
        UserId    = Guid.NewGuid(),
        TenantId  = TenantId,
        Username  = $"user{index}",
        Email     = $"user{index}@example.com",
        FirstName = $"First{index}",
        LastName  = $"Last{index}",
        Status    = UserStatus.Active
    };

    [Fact]
    public async Task Handle_ShouldReturnAllUsersForTenant()
    {
        var users = new List<User> { MakeUser(1), MakeUser(2) };

        _userRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(users);
        _userRoleRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<UserRole>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetUsersQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenNoUsers_ShouldReturnEmpty()
    {
        _userRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<User>());
        _userRoleRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<UserRole>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetUsersQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldIncludeRoleNamesInDto()
    {
        var user   = MakeUser(1);
        var roleId = Guid.NewGuid();
        var role   = new Role { RoleId = roleId, TenantId = TenantId, RoleName = "Admin" };
        var userRole = new UserRole { UserRoleId = Guid.NewGuid(), UserId = user.UserId, RoleId = roleId };

        _userRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<User> { user });
        _userRoleRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<UserRole> { userRole });
        _roleRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Role> { role });

        var handler = CreateHandler();
        var result  = (await handler.Handle(new GetUsersQuery(), CancellationToken.None)).First();

        result.Roles.Should().Contain("Admin");
    }

    [Fact]
    public async Task Handle_WhenUserHasNoRoles_ShouldReturnEmptyRolesList()
    {
        var user = MakeUser(1);

        _userRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<User> { user });
        _userRoleRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<UserRole>());

        var handler = CreateHandler();
        var result  = (await handler.Handle(new GetUsersQuery(), CancellationToken.None)).First();

        result.Roles.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldUseCurrentTenantId()
    {
        _userRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<User>());
        _userRoleRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<UserRole>());

        var handler = CreateHandler();
        await handler.Handle(new GetUsersQuery(), CancellationToken.None);

        _tenantContext.Verify(t => t.GetCurrentTenantId(), Times.Once);
    }
}
