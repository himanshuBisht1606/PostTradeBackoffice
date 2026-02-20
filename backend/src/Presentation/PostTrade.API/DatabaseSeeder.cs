using Microsoft.EntityFrameworkCore;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;
using PostTrade.Persistence.Context;

namespace PostTrade.API;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostTradeDbContext>();

        // Apply any pending migrations automatically
        await context.Database.MigrateAsync();

        // Skip if already seeded
        if (await context.Set<Tenant>().AnyAsync())
        {
            logger.LogInformation("Seed data already exists. Skipping.");
            return;
        }

        logger.LogInformation("Seeding test data...");

        var now = DateTime.UtcNow;

        var tenantId = Guid.NewGuid();
        var roleId   = Guid.NewGuid();
        var userId   = Guid.NewGuid();

        // ── Tenant ───────────────────────────────────────────────────────────
        var tenant = new Tenant
        {
            TenantId      = tenantId,
            TenantCode    = "DEMO",
            TenantName    = "Demo Brokerage",
            ContactEmail  = "admin@demobrokerage.com",
            ContactPhone  = "+91-9000000000",
            Status        = TenantStatus.Active,
            CreatedAt     = now,
            CreatedBy     = "seeder"
        };
        context.Set<Tenant>().Add(tenant);

        // ── Role ─────────────────────────────────────────────────────────────
        var role = new Role
        {
            RoleId       = roleId,
            TenantId     = tenantId,
            RoleName     = "Admin",
            Description  = "Full system access",
            IsSystemRole = true,
            CreatedAt    = now,
            CreatedBy    = "seeder"
        };
        context.Set<Role>().Add(role);

        // ── User ─────────────────────────────────────────────────────────────
        var user = new User
        {
            UserId       = userId,
            TenantId     = tenantId,
            Username     = "admin",
            Email        = "admin@demobrokerage.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            FirstName    = "System",
            LastName     = "Admin",
            Status       = UserStatus.Active,
            CreatedAt    = now,
            CreatedBy    = "seeder"
        };
        context.Set<User>().Add(user);

        // ── UserRole ──────────────────────────────────────────────────────────
        context.Set<UserRole>().Add(new UserRole
        {
            UserRoleId   = Guid.NewGuid(),
            UserId       = userId,
            RoleId       = roleId,
            AssignedDate = now,
            CreatedAt    = now,
            CreatedBy    = "seeder"
        });

        await context.SaveChangesAsync();

        logger.LogInformation(
            "Seed complete — TenantCode: DEMO | Username: admin | Password: Admin@123");
    }
}
