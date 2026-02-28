using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Persistence.Configurations.Master;

public class ExchangeSegmentConfiguration : IEntityTypeConfiguration<ExchangeSegment>
{
    public void Configure(EntityTypeBuilder<ExchangeSegment> builder)
    {
        builder.ToTable("ExchangeSegments", "master");
        builder.HasKey(e => e.ExchangeSegmentId);

        builder.Property(e => e.ExchangeSegmentCode).IsRequired().HasMaxLength(50);
        builder.Property(e => e.ExchangeSegmentName).IsRequired().HasMaxLength(200);

        builder.HasIndex(e => new { e.TenantId, e.ExchangeId, e.ExchangeSegmentCode }).IsUnique();

        builder.HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId);
        builder.HasOne(e => e.Exchange).WithMany(ex => ex.ExchangeSegments).HasForeignKey(e => e.ExchangeId);
        builder.HasOne(e => e.Segment).WithMany().HasForeignKey(e => e.SegmentId);
    }
}
