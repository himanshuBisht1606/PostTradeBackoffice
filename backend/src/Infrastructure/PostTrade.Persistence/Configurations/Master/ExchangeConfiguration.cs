using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Persistence.Configurations.Master;

public class ExchangeConfiguration : IEntityTypeConfiguration<Exchange>
{
    public void Configure(EntityTypeBuilder<Exchange> builder)
    {
        builder.ToTable("Exchanges", "master");
        builder.HasKey(e => e.ExchangeId);

        builder.Property(e => e.ExchangeCode).IsRequired().HasMaxLength(20);
        builder.Property(e => e.ExchangeName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Country).IsRequired().HasMaxLength(100);
        builder.Property(e => e.TimeZone).HasMaxLength(100);

        builder.HasIndex(e => new { e.TenantId, e.ExchangeCode }).IsUnique();

        builder.HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId);
        builder.HasMany(e => e.ExchangeSegments).WithOne(s => s.Exchange).HasForeignKey(s => s.ExchangeId);
    }
}
