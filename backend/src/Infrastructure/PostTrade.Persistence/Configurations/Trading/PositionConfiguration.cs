using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.Trading;

namespace PostTrade.Persistence.Configurations.Trading;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("Positions", "trading");
        builder.HasKey(p => p.PositionId);

        builder.Property(p => p.AverageBuyPrice).HasColumnType("decimal(18,4)");
        builder.Property(p => p.AverageSellPrice).HasColumnType("decimal(18,4)");
        builder.Property(p => p.LastTradePrice).HasColumnType("decimal(18,4)");
        builder.Property(p => p.MarketPrice).HasColumnType("decimal(18,4)");
        builder.Property(p => p.RealizedPnL).HasColumnType("decimal(18,4)");
        builder.Property(p => p.UnrealizedPnL).HasColumnType("decimal(18,4)");
        builder.Property(p => p.DayPnL).HasColumnType("decimal(18,4)");
        builder.Property(p => p.TotalPnL).HasColumnType("decimal(18,4)");
        builder.Property(p => p.BuyValue).HasColumnType("decimal(18,4)");
        builder.Property(p => p.SellValue).HasColumnType("decimal(18,4)");
        builder.Property(p => p.NetValue).HasColumnType("decimal(18,4)");

        builder.HasIndex(p => new { p.TenantId, p.PositionDate });
        builder.HasIndex(p => new { p.TenantId, p.ClientId, p.InstrumentId, p.PositionDate }).IsUnique();
    }
}
