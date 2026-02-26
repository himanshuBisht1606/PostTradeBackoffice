using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Persistence.Configurations.Master;

public class ClientSegmentActivationConfiguration : IEntityTypeConfiguration<ClientSegmentActivation>
{
    public void Configure(EntityTypeBuilder<ClientSegmentActivation> builder)
    {
        builder.ToTable("ClientSegmentActivations", "master");
        builder.HasKey(a => a.ActivationId);

        builder.Property(a => a.ExposureLimit).HasColumnType("numeric(18,4)");

        builder.HasIndex(a => new { a.TenantId, a.ClientId, a.ExchangeSegmentId }).IsUnique();

        builder.HasOne(a => a.Tenant).WithMany().HasForeignKey(a => a.TenantId);
        builder.HasOne(a => a.Client).WithMany(c => c.SegmentActivations).HasForeignKey(a => a.ClientId);
        builder.HasOne(a => a.ExchangeSegment).WithMany(e => e.ClientSegmentActivations).HasForeignKey(a => a.ExchangeSegmentId);
    }
}
