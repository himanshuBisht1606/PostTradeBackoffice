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

        // ── State master (global reference — seed once) ───────────────────────
        // Source: state.txt  columns: COUNTRY_ID,STATE_ID,STATE_NAME,NSE,BSE,CVL,NDML,NCDEX,NSEKRA,NSDL
        // To refresh: TRUNCATE TABLE reference."StateMaster"; then restart the API.
        if (!await db.StateMasters.AnyAsync())
        {
            var states = new List<StateMaster>
            {
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "AN",    StateName = "Andaman & Nicobar",                           NseCode = 1,  BseName = "ANDAMAN AND NICOBAR",                          CvlCode = 35, NdmlCode = 1,    NcdexCode = 4,    NseKraCode = 1,    NsdlCode = 35, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "AP",    StateName = "Andhra Pradesh",                              NseCode = 2,  BseName = "ANDHRA PRADESH",                               CvlCode = 28, NdmlCode = 2,    NcdexCode = 3,    NseKraCode = 2,    NsdlCode = 37, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "AR",    StateName = "Arunachal Pradesh",                           NseCode = 3,  BseName = "ARUNACHAL PRADESH",                            CvlCode = 12, NdmlCode = 3,    NcdexCode = 1,    NseKraCode = 3,    NsdlCode = 12, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "AS",    StateName = "Assam",                                       NseCode = 4,  BseName = "ASSAM",                                        CvlCode = 13, NdmlCode = 4,    NcdexCode = 2,    NseKraCode = 4,    NsdlCode = 18, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "BR",    StateName = "Bihar",                                       NseCode = 5,  BseName = "BIHAR",                                        CvlCode = 10, NdmlCode = 5,    NcdexCode = 5,    NseKraCode = 5,    NsdlCode = 10, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "CH",    StateName = "Chandigarh",                                  NseCode = 6,  BseName = "CHANDIGARH",                                   CvlCode = 4,  NdmlCode = 6,    NcdexCode = 6,    NseKraCode = 6,    NsdlCode = 4,  IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "CG",    StateName = "Chattisgarh",                                 NseCode = 33, BseName = "CHHATTISGARH",                                 CvlCode = 22, NdmlCode = 33,   NcdexCode = 7,    NseKraCode = 33,   NsdlCode = 22, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "LA",    StateName = "Ladakh",                                      NseCode = 38, BseName = "LADAKH",                                       CvlCode = null, NdmlCode = null, NcdexCode = null, NseKraCode = null, NsdlCode = 38, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "DNHDD", StateName = "Dadra and Nagar Haveli and Daman & Diu",      NseCode = 7,  BseName = "DADRA AND NAGAR HAVELI AND DAMAN AND DIU",     CvlCode = 26, NdmlCode = 7,    NcdexCode = 10,   NseKraCode = 7,    NsdlCode = 26, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "DL",    StateName = "Delhi",                                       NseCode = 9,  BseName = "DELHI",                                        CvlCode = 7,  NdmlCode = 9,    NcdexCode = 8,    NseKraCode = 9,    NsdlCode = 7,  IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "GA",    StateName = "Goa",                                         NseCode = 10, BseName = "GOA",                                          CvlCode = 30, NdmlCode = 10,   NcdexCode = 12,   NseKraCode = 10,   NsdlCode = 30, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "GJ",    StateName = "Gujarat",                                     NseCode = 11, BseName = "GUJARAT",                                      CvlCode = 24, NdmlCode = 11,   NcdexCode = 11,   NseKraCode = 11,   NsdlCode = 24, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "HR",    StateName = "Haryana",                                     NseCode = 12, BseName = "HARYANA",                                      CvlCode = 6,  NdmlCode = 12,   NcdexCode = 14,   NseKraCode = 12,   NsdlCode = 6,  IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "HP",    StateName = "Himachal Pradesh",                            NseCode = 13, BseName = "HIMACHAL PRADESH",                             CvlCode = 2,  NdmlCode = 13,   NcdexCode = 13,   NseKraCode = 13,   NsdlCode = 2,  IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "JK",    StateName = "Jammu & Kashmir",                             NseCode = 14, BseName = "JAMMU AND KASHMIR",                            CvlCode = 1,  NdmlCode = 14,   NcdexCode = 15,   NseKraCode = 14,   NsdlCode = 1,  IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "JH",    StateName = "Jharkhand",                                   NseCode = 35, BseName = "JHARKHAND",                                    CvlCode = 20, NdmlCode = 35,   NcdexCode = 16,   NseKraCode = 35,   NsdlCode = 20, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "KA",    StateName = "Karnataka",                                   NseCode = 15, BseName = "KARNATAKA",                                    CvlCode = 29, NdmlCode = 15,   NcdexCode = 17,   NseKraCode = 15,   NsdlCode = 29, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "KL",    StateName = "Kerala",                                      NseCode = 16, BseName = "KERALA",                                       CvlCode = 32, NdmlCode = 16,   NcdexCode = 18,   NseKraCode = 16,   NsdlCode = 32, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "LD",    StateName = "Lakshadweep",                                 NseCode = 17, BseName = "LAKSHADWEEP",                                  CvlCode = 31, NdmlCode = 17,   NcdexCode = 19,   NseKraCode = 17,   NsdlCode = 31, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "MP",    StateName = "Madhya Pradesh",                              NseCode = 18, BseName = "MADHYA PRADESH",                               CvlCode = 23, NdmlCode = 18,   NcdexCode = 23,   NseKraCode = 18,   NsdlCode = 23, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "MH",    StateName = "Maharashtra",                                 NseCode = 19, BseName = "MAHARASHTRA",                                  CvlCode = 27, NdmlCode = 19,   NcdexCode = 24,   NseKraCode = 19,   NsdlCode = 27, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "MN",    StateName = "Manipur",                                     NseCode = 20, BseName = "MANIPUR",                                      CvlCode = 14, NdmlCode = 20,   NcdexCode = 20,   NseKraCode = 20,   NsdlCode = 14, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "ML",    StateName = "Meghalaya",                                   NseCode = 21, BseName = "MEGHALAYA",                                    CvlCode = 17, NdmlCode = 21,   NcdexCode = 22,   NseKraCode = 21,   NsdlCode = 17, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "MZ",    StateName = "Mizoram",                                     NseCode = 22, BseName = "MIZORAM",                                      CvlCode = 15, NdmlCode = 22,   NcdexCode = 21,   NseKraCode = 22,   NsdlCode = 15, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "NL",    StateName = "Nagaland",                                    NseCode = 23, BseName = "NAGALAND",                                     CvlCode = 18, NdmlCode = 23,   NcdexCode = 25,   NseKraCode = 23,   NsdlCode = 13, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "OR",    StateName = "Orissa",                                      NseCode = 24, BseName = "ODISHA",                                       CvlCode = 21, NdmlCode = 24,   NcdexCode = 26,   NseKraCode = 24,   NsdlCode = 21, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "PY",    StateName = "Pondicherry",                                 NseCode = 25, BseName = "PONDICHERRY",                                  CvlCode = 34, NdmlCode = 25,   NcdexCode = 28,   NseKraCode = 25,   NsdlCode = 34, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "PB",    StateName = "Punjab",                                      NseCode = 26, BseName = "PUNJAB",                                       CvlCode = 3,  NdmlCode = 26,   NcdexCode = 27,   NseKraCode = 26,   NsdlCode = 3,  IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "RJ",    StateName = "Rajasthan",                                   NseCode = 27, BseName = "RAJASTHAN",                                    CvlCode = 8,  NdmlCode = 27,   NcdexCode = 29,   NseKraCode = 27,   NsdlCode = 8,  IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "SK",    StateName = "Sikkim",                                      NseCode = 28, BseName = "SIKKIM",                                       CvlCode = 11, NdmlCode = 28,   NcdexCode = 30,   NseKraCode = 28,   NsdlCode = 11, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "TN",    StateName = "Tamil Nadu",                                  NseCode = 29, BseName = "TAMIL NADU",                                   CvlCode = 33, NdmlCode = 29,   NcdexCode = 32,   NseKraCode = 29,   NsdlCode = 33, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "TS",    StateName = "Telangana",                                   NseCode = 37, BseName = "TELANGANA",                                    CvlCode = 37, NdmlCode = null, NcdexCode = null, NseKraCode = null, NsdlCode = 36, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "TR",    StateName = "Tripura",                                     NseCode = 30, BseName = "TRIPURA",                                      CvlCode = 16, NdmlCode = 30,   NcdexCode = 31,   NseKraCode = 30,   NsdlCode = 16, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "UP",    StateName = "Uttar Pradesh",                               NseCode = 31, BseName = "UTTAR PRADESH",                                CvlCode = 9,  NdmlCode = 31,   NcdexCode = 34,   NseKraCode = 31,   NsdlCode = 9,  IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "UA",    StateName = "Uttarakhand",                                 NseCode = 34, BseName = "UTTARAKHAND",                                  CvlCode = 5,  NdmlCode = 34,   NcdexCode = 33,   NseKraCode = 34,   NsdlCode = 5,  IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "WB",    StateName = "West Bengal",                                 NseCode = 32, BseName = "WEST BENGAL",                                  CvlCode = 19, NdmlCode = 32,   NcdexCode = 35,   NseKraCode = 32,   NsdlCode = 19, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
                new() { StateId = Guid.NewGuid(), CountryId = "IN", StateCode = "XX",    StateName = "OTHER",                                       NseCode = 99, BseName = "OTHERS",                                       CvlCode = 99, NdmlCode = 99,   NcdexCode = null, NseKraCode = 99,   NsdlCode = 99, IsActive = true, CreatedAt = now, CreatedBy = "seed" },
            };
            await db.StateMasters.AddRangeAsync(states);
            await db.SaveChangesAsync();
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
