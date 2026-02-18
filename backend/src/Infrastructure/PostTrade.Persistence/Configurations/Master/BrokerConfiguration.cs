using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Persistence.Configurations.Master;

public class BrokerConfiguration : IEntityTypeConfiguration<Broker>
{
    public void Configure(EntityTypeBuilder<Broker> builder)
    {
        builder.ToTable("Brokers", "master");
        builder.HasKey(b => b.BrokerId);

        builder.Property(b => b.BrokerCode).IsRequired().HasMaxLength(20);
        builder.Property(b => b.BrokerName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.SEBIRegistrationNo).HasMaxLength(50);
        builder.Property(b => b.ContactEmail).IsRequired().HasMaxLength(200);
        builder.Property(b => b.ContactPhone).HasMaxLength(20);
        builder.Property(b => b.Address).HasMaxLength(500);
        builder.Property(b => b.PAN).HasMaxLength(20);
        builder.Property(b => b.GST).HasMaxLength(20);

        builder.HasIndex(b => new { b.TenantId, b.BrokerCode }).IsUnique();

        builder.HasOne(b => b.Tenant).WithMany(t => t.Brokers).HasForeignKey(b => b.TenantId);
        builder.HasMany(b => b.Clients).WithOne(c => c.Broker).HasForeignKey(c => c.BrokerId);
    }
}
