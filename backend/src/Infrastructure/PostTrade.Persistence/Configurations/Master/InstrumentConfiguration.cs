using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Persistence.Configurations.Master;

public class InstrumentConfiguration : IEntityTypeConfiguration<Instrument>
{
    public void Configure(EntityTypeBuilder<Instrument> builder)
    {
        builder.ToTable("Instruments", "master");
        builder.HasKey(i => i.InstrumentId);

        builder.Property(i => i.InstrumentCode).IsRequired().HasMaxLength(50);
        builder.Property(i => i.InstrumentName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Symbol).IsRequired().HasMaxLength(50);
        builder.Property(i => i.ISIN).HasMaxLength(20);
        builder.Property(i => i.Series).HasMaxLength(10);
        builder.Property(i => i.LotSize).HasColumnType("decimal(18,4)");
        builder.Property(i => i.TickSize).HasColumnType("decimal(18,4)");
        builder.Property(i => i.StrikePrice).HasColumnType("decimal(18,4)");

        builder.HasIndex(i => new { i.TenantId, i.InstrumentCode }).IsUnique();
        builder.HasIndex(i => new { i.TenantId, i.Symbol, i.ExchangeId });
        builder.HasIndex(i => i.ISIN);

        builder.HasOne(i => i.Tenant).WithMany().HasForeignKey(i => i.TenantId);
        builder.HasOne(i => i.Exchange).WithMany().HasForeignKey(i => i.ExchangeId);
        builder.HasOne(i => i.Segment).WithMany().HasForeignKey(i => i.SegmentId);
    }
}
