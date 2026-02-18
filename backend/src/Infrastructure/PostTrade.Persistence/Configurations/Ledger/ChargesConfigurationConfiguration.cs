using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.Ledger;

namespace PostTrade.Persistence.Configurations.Ledger;

public class ChargesConfigurationEntityConfiguration : IEntityTypeConfiguration<ChargesConfiguration>
{
    public void Configure(EntityTypeBuilder<ChargesConfiguration> builder)
    {
        builder.ToTable("ChargesConfigurations", "ledger");
        builder.HasKey(c => c.ChargesConfigId);

        builder.Property(c => c.ChargeName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Rate).HasColumnType("decimal(18,6)");
        builder.Property(c => c.MinAmount).HasColumnType("decimal(18,4)");
        builder.Property(c => c.MaxAmount).HasColumnType("decimal(18,4)");

        builder.HasIndex(c => new { c.TenantId, c.BrokerId, c.ChargeType, c.EffectiveFrom });
    }
}
