using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.Trading;

namespace PostTrade.Persistence.Configurations.Trading;

public class PnLSnapshotConfiguration : IEntityTypeConfiguration<PnLSnapshot>
{
    public void Configure(EntityTypeBuilder<PnLSnapshot> builder)
    {
        builder.ToTable("PnLSnapshots", "trading");
        builder.HasKey(p => p.PnLId);

        builder.Property(p => p.RealizedPnL).HasColumnType("decimal(18,4)");
        builder.Property(p => p.UnrealizedPnL).HasColumnType("decimal(18,4)");
        builder.Property(p => p.TotalPnL).HasColumnType("decimal(18,4)");
        builder.Property(p => p.Brokerage).HasColumnType("decimal(18,4)");
        builder.Property(p => p.Taxes).HasColumnType("decimal(18,4)");
        builder.Property(p => p.NetPnL).HasColumnType("decimal(18,4)");
        builder.Property(p => p.AveragePrice).HasColumnType("decimal(18,4)");
        builder.Property(p => p.MarketPrice).HasColumnType("decimal(18,4)");

        builder.HasIndex(p => new { p.TenantId, p.SnapshotDate });
        builder.HasIndex(p => new { p.TenantId, p.ClientId, p.InstrumentId, p.SnapshotDate });
    }
}
