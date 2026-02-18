using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Persistence.Configurations.Master;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants", "master");
        builder.HasKey(t => t.TenantId);

        builder.Property(t => t.TenantCode).IsRequired().HasMaxLength(20);
        builder.Property(t => t.TenantName).IsRequired().HasMaxLength(200);
        builder.Property(t => t.ContactEmail).IsRequired().HasMaxLength(200);
        builder.Property(t => t.ContactPhone).HasMaxLength(20);
        builder.Property(t => t.LicenseKey).HasMaxLength(200);
        builder.Property(t => t.Address).HasMaxLength(500);
        builder.Property(t => t.City).HasMaxLength(100);
        builder.Property(t => t.Country).HasMaxLength(100);
        builder.Property(t => t.TaxId).HasMaxLength(50);

        builder.HasIndex(t => t.TenantCode).IsUnique();

        builder.HasMany(t => t.Brokers).WithOne(b => b.Tenant).HasForeignKey(b => b.TenantId);
        builder.HasMany(t => t.Users).WithOne(u => u.Tenant).HasForeignKey(u => u.TenantId);
    }
}
