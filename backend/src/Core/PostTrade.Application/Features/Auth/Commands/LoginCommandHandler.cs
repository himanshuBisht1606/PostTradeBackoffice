using MediatR;
using PostTrade.Application.Features.Auth.DTOs;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IRepository<Tenant> _tenantRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<UserRole> _userRoleRepo;
    private readonly IRepository<Role> _roleRepo;
    private readonly IJwtService _jwtService;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IRepository<Tenant> tenantRepo,
        IRepository<User> userRepo,
        IRepository<UserRole> userRoleRepo,
        IRepository<Role> roleRepo,
        IJwtService jwtService,
        IPasswordService passwordService,
        IUnitOfWork unitOfWork)
    {
        _tenantRepo = tenantRepo;
        _userRepo = userRepo;
        _userRoleRepo = userRoleRepo;
        _roleRepo = roleRepo;
        _jwtService = jwtService;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Find active tenant by TenantCode
        var tenants = await _tenantRepo.FindAsync(
            t => t.TenantCode == request.TenantCode && t.Status == TenantStatus.Active,
            cancellationToken);

        var tenant = tenants.FirstOrDefault()
            ?? throw new UnauthorizedAccessException("Invalid tenant or credentials.");

        // 2. Find user by username within that tenant
        var users = await _userRepo.FindAsync(
            u => u.Username == request.Username && u.TenantId == tenant.TenantId,
            cancellationToken);

        var user = users.FirstOrDefault()
            ?? throw new UnauthorizedAccessException("Invalid username or password.");

        // 3. Check user status
        if (user.Status != UserStatus.Active)
            throw new UnauthorizedAccessException("Account is not active.");

        if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
            throw new UnauthorizedAccessException("Account is locked. Please try again later.");

        // 4. Verify password
        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username or password.");

        // 5. Load roles
        var userRoles = await _userRoleRepo.FindAsync(
            ur => ur.UserId == user.UserId,
            cancellationToken);

        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();

        var roles = roleIds.Count > 0
            ? await _roleRepo.FindAsync(r => roleIds.Contains(r.RoleId), cancellationToken)
            : Enumerable.Empty<Role>();

        var roleNames = roles.Select(r => r.RoleName).ToList();

        // 6. Update last login timestamp
        user.LastLoginAt = DateTime.UtcNow;
        user.FailedLoginAttempts = 0;
        await _userRepo.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Generate JWT and return
        var expiryMinutes = 480;
        var token = _jwtService.GenerateToken(user.UserId, tenant.TenantId, user.Username, roleNames);

        return new LoginResponse(
            Token: token,
            ExpiresAt: DateTime.UtcNow.AddMinutes(expiryMinutes),
            Username: user.Username,
            Roles: roleNames
        );
    }
}
