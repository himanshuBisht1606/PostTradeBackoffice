using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Persistence.Configurations.PostTradeProcessing;

public class CmScripMasterConfiguration : IEntityTypeConfiguration<CmScripMaster>
{
    public void Configure(EntityTypeBuilder<CmScripMaster> builder)
    {
        builder.ToTable("CmScripMasters", "post_trade");

        builder.HasKey(s => s.CmScripMasterId);

        builder.Property(s => s.Exchange).IsRequired().HasMaxLength(10);
        builder.Property(s => s.Symbol).IsRequired().HasMaxLength(20);
        builder.Property(s => s.ISIN).IsRequired().HasMaxLength(15);
        builder.Property(s => s.Series).HasMaxLength(10);
        builder.Property(s => s.Name).HasMaxLength(100);
        builder.Property(s => s.FaceValue).HasColumnType("decimal(10,4)");
        builder.Property(s => s.TickSize).HasColumnType("decimal(10,4)");
        builder.Property(s => s.InstrumentType).HasMaxLength(20);

        builder.HasIndex(s => new { s.TenantId, s.Exchange, s.TradingDate, s.ISIN }).IsUnique();
        builder.HasIndex(s => new { s.TenantId, s.Exchange, s.TradingDate });
    }
}
