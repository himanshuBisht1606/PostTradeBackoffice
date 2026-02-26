using Microsoft.EntityFrameworkCore;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;
using PostTrade.Persistence.Context;

namespace PostTrade.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(PostTradeDbContext db, Func<string, string> hashPassword)
    {
        var now = DateTime.UtcNow;

        // ── Tenant + users (only once) ────────────────────────────────────────
        Guid tenantId;
        if (!await db.Tenants.AnyAsync())
        {
            tenantId     = Guid.NewGuid();
            var roleId   = Guid.NewGuid();
            var userId   = Guid.NewGuid();

            var tenant = new Tenant
            {
                TenantId      = tenantId,
                TenantCode    = "DEMO",
                TenantName    = "Demo Tenant",
                ContactEmail  = "admin@default.com",
                ContactPhone  = "0000000000",
                Status        = TenantStatus.Active,
                CreatedAt     = now,
                CreatedBy     = "seed",
                Version       = 1
            };

            var adminRole = new Role
            {
                RoleId       = roleId,
                TenantId     = tenantId,
                RoleName     = "PlatformSuperAdmin",
                Description  = "Platform super administrator",
                IsSystemRole = true,
                CreatedAt    = now,
                CreatedBy    = "seed"
            };

            var adminUser = new User
            {
                UserId       = userId,
                TenantId     = tenantId,
                Username     = "admin",
                Email        = "admin@default.com",
                PasswordHash = hashPassword("Admin@123"),
                FirstName    = "System",
                LastName     = "Admin",
                Status       = UserStatus.Active,
                CreatedAt    = now,
                CreatedBy    = "seed"
            };

            var userRole = new UserRole
            {
                UserRoleId   = Guid.NewGuid(),
                UserId       = userId,
                RoleId       = roleId,
                AssignedDate = now,
                CreatedAt    = now,
                CreatedBy    = "seed"
            };

            await db.Tenants.AddAsync(tenant);
            await db.Roles.AddAsync(adminRole);
            await db.Users.AddAsync(adminUser);
            await db.UserRoles.AddAsync(userRole);
            await db.SaveChangesAsync();
        }
        else
        {
            tenantId = (await db.Tenants.FirstAsync()).TenantId;
        }

        // ── Default broker (seed once per tenant) ────────────────────────────
        if (!await db.Brokers.AnyAsync(b => b.TenantId == tenantId))
        {
            var broker = new Broker
            {
                BrokerId     = Guid.NewGuid(),
                TenantId     = tenantId,
                BrokerCode   = "BRK001",
                BrokerName   = "Demo Broker",
                ContactEmail = "broker@demo.com",
                ContactPhone = "0000000000",
                Status       = BrokerStatus.Active,
                CreatedAt    = now,
                CreatedBy    = "seed",
                Version      = 1
            };

            await db.Brokers.AddAsync(broker);
            await db.SaveChangesAsync();
        }
    }
}
