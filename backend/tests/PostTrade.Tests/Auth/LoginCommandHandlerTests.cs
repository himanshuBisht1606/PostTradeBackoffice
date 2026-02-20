using PostTrade.Application.Features.Auth.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IRepository<Tenant>>   _tenantRepo   = new();
    private readonly Mock<IRepository<User>>     _userRepo     = new();
    private readonly Mock<IRepository<UserRole>> _userRoleRepo = new();
    private readonly Mock<IRepository<Role>>     _roleRepo     = new();
    private readonly Mock<IJwtService>           _jwtService   = new();
    private readonly Mock<IPasswordService>      _passwordService = new();
    private readonly Mock<IUnitOfWork>           _unitOfWork   = new();

    private LoginCommandHandler CreateHandler() => new(
        _tenantRepo.Object, _userRepo.Object, _userRoleRepo.Object, _roleRepo.Object,
        _jwtService.Object, _passwordService.Object, _unitOfWork.Object);

    // ── Test data builders ────────────────────────────────────────────────────

    private static Tenant ActiveTenant(string code = "DEMO") => new()
    {
        TenantId   = Guid.NewGuid(),
        TenantCode = code,
        TenantName = "Demo Brokerage",
        Status     = TenantStatus.Active
    };

    private static User ActiveUser(Guid tenantId, string username = "admin") => new()
    {
        UserId       = Guid.NewGuid(),
        TenantId     = tenantId,
        Username     = username,
        PasswordHash = "hashed-password",
        Status       = UserStatus.Active,
        LockedUntil  = null
    };

    // ── Helper: return empty collections for role-related repos ──────────────

    private void SetupEmptyRoles()
    {
        _userRoleRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserRole>());
        _roleRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role>());
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenTenantNotFound_ShouldThrowUnauthorizedAccessException()
    {
        _tenantRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenant, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tenant>());

        var handler = CreateHandler();
        var act = () => handler.Handle(new LoginCommand("admin", "Admin@123", "WRONG"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid tenant*");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowUnauthorizedAccessException()
    {
        var tenant = ActiveTenant();
        _tenantRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenant, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tenant> { tenant });
        _userRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        var handler = CreateHandler();
        var act = () => handler.Handle(new LoginCommand("nobody", "Admin@123", "DEMO"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid username or password*");
    }

    [Fact]
    public async Task Handle_WhenUserIsInactive_ShouldThrowUnauthorizedAccessException()
    {
        var tenant = ActiveTenant();
        var user   = ActiveUser(tenant.TenantId);
        user.Status = UserStatus.Inactive;

        _tenantRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenant, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tenant> { tenant });
        _userRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });

        var handler = CreateHandler();
        var act = () => handler.Handle(new LoginCommand("admin", "Admin@123", "DEMO"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not active*");
    }

    [Fact]
    public async Task Handle_WhenAccountIsLocked_ShouldThrowUnauthorizedAccessException()
    {
        var tenant = ActiveTenant();
        var user   = ActiveUser(tenant.TenantId);
        user.LockedUntil = DateTime.UtcNow.AddHours(1); // locked for an hour

        _tenantRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenant, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tenant> { tenant });
        _userRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });

        var handler = CreateHandler();
        var act = () => handler.Handle(new LoginCommand("admin", "Admin@123", "DEMO"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*locked*");
    }

    [Fact]
    public async Task Handle_WhenPasswordIsWrong_ShouldThrowUnauthorizedAccessException()
    {
        var tenant = ActiveTenant();
        var user   = ActiveUser(tenant.TenantId);

        _tenantRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenant, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tenant> { tenant });
        _userRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });
        _passwordService
            .Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        var handler = CreateHandler();
        var act = () => handler.Handle(new LoginCommand("admin", "WrongPass", "DEMO"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid username or password*");
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnLoginResponse()
    {
        var tenant = ActiveTenant();
        var user   = ActiveUser(tenant.TenantId);
        var role   = new Role { RoleId = Guid.NewGuid(), RoleName = "Admin", TenantId = tenant.TenantId };
        var userRole = new UserRole { UserId = user.UserId, RoleId = role.RoleId, UserRoleId = Guid.NewGuid() };

        _tenantRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenant, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tenant> { tenant });
        _userRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });
        _passwordService
            .Setup(p => p.VerifyPassword("Admin@123", user.PasswordHash))
            .Returns(true);
        _userRoleRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserRole> { userRole });
        _roleRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role> { role });
        _jwtService
            .Setup(j => j.GenerateToken(user.UserId, tenant.TenantId, user.Username, It.IsAny<IEnumerable<string>>()))
            .Returns("jwt-token");
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = CreateHandler();
        var result  = await handler.Handle(new LoginCommand("admin", "Admin@123", "DEMO"), CancellationToken.None);

        result.Should().NotBeNull();
        result.Token.Should().Be("jwt-token");
        result.Username.Should().Be(user.Username);
        result.Roles.Should().Contain("Admin");
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldUpdateLastLoginAndSave()
    {
        var tenant = ActiveTenant();
        var user   = ActiveUser(tenant.TenantId);

        _tenantRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenant, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tenant> { tenant });
        _userRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });
        _passwordService
            .Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        _jwtService
            .Setup(j => j.GenerateToken(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
            .Returns("jwt-token");
        SetupEmptyRoles();
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var before  = DateTime.UtcNow;
        var handler = CreateHandler();
        await handler.Handle(new LoginCommand("admin", "Admin@123", "DEMO"), CancellationToken.None);

        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeOnOrAfter(before);
        user.FailedLoginAttempts.Should().Be(0);

        _userRepo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoRoles_ShouldReturnEmptyRoles()
    {
        var tenant = ActiveTenant();
        var user   = ActiveUser(tenant.TenantId);

        _tenantRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenant, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tenant> { tenant });
        _userRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });
        _passwordService
            .Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        _jwtService
            .Setup(j => j.GenerateToken(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
            .Returns("jwt-token");
        SetupEmptyRoles();
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = CreateHandler();
        var result  = await handler.Handle(new LoginCommand("admin", "Admin@123", "DEMO"), CancellationToken.None);

        result.Roles.Should().BeEmpty();
    }
}
