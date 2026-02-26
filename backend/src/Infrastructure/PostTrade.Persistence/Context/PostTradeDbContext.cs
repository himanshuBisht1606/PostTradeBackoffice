using Microsoft.EntityFrameworkCore;
using PostTrade.Domain.Entities;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Entities.Trading;
using PostTrade.Domain.Entities.Settlement;
using PostTrade.Domain.Entities.Ledger;
using PostTrade.Domain.Entities.Reconciliation;
using PostTrade.Domain.Entities.CorporateActions;
using PostTrade.Domain.Entities.Audit;
using PostTrade.Application.Interfaces;

namespace PostTrade.Persistence.Context;

public class PostTradeDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public PostTradeDbContext(
        DbContextOptions<PostTradeDbContext> options,
        ITenantContext tenantContext) : base(options)
    {
        _tenantContext = tenantContext;
    }

    // Master Data
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Broker> Brokers => Set<Broker>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Instrument> Instruments => Set<Instrument>();
    public DbSet<Exchange> Exchanges => Set<Exchange>();
    public DbSet<Segment> Segments => Set<Segment>();
    public DbSet<ExchangeSegment> ExchangeSegments => Set<ExchangeSegment>();
    public DbSet<ClientSegmentActivation> ClientSegmentActivations => Set<ClientSegmentActivation>();

    // Trading
    public DbSet<Trade> Trades => Set<Trade>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<PnLSnapshot> PnLSnapshots => Set<PnLSnapshot>();

    // Settlement
    public DbSet<SettlementBatch> SettlementBatches => Set<SettlementBatch>();
    public DbSet<SettlementObligation> SettlementObligations => Set<SettlementObligation>();

    // Ledger
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<ChargesConfiguration> ChargesConfigurations => Set<ChargesConfiguration>();

    // Reconciliation
    public DbSet<Domain.Entities.Reconciliation.Reconciliation> Reconciliations => Set<Domain.Entities.Reconciliation.Reconciliation>();
    public DbSet<ReconException> ReconExceptions => Set<ReconException>();

    // Corporate Actions
    public DbSet<CorporateAction> CorporateActions => Set<CorporateAction>();

    // Audit
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PostTradeDbContext).Assembly);

        // Global query filter for multi-tenancy
        // tenantId may be Guid.Empty at design-time (migrations); skip filters in that case
        Guid tenantId = Guid.Empty;
        try { tenantId = _tenantContext.GetCurrentTenantId(); } catch { }

        if (tenantId != Guid.Empty)
        {
            modelBuilder.Entity<Broker>().HasQueryFilter(e => e.TenantId == tenantId);
            modelBuilder.Entity<Client>().HasQueryFilter(e => e.TenantId == tenantId);
            modelBuilder.Entity<Branch>().HasQueryFilter(e => e.TenantId == tenantId);
            modelBuilder.Entity<ExchangeSegment>().HasQueryFilter(e => e.TenantId == tenantId);
            modelBuilder.Entity<ClientSegmentActivation>().HasQueryFilter(e => e.TenantId == tenantId);
            modelBuilder.Entity<User>().HasQueryFilter(e => e.TenantId == tenantId);
            modelBuilder.Entity<Trade>().HasQueryFilter(e => e.TenantId == tenantId);
            modelBuilder.Entity<Position>().HasQueryFilter(e => e.TenantId == tenantId);
            modelBuilder.Entity<SettlementBatch>().HasQueryFilter(e => e.TenantId == tenantId);
            modelBuilder.Entity<LedgerEntry>().HasQueryFilter(e => e.TenantId == tenantId);
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
