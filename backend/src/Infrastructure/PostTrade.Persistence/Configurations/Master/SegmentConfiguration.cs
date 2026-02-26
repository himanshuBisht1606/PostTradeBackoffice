using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Persistence.Configurations.Master;

public class SegmentConfiguration : IEntityTypeConfiguration<Segment>
{
    public void Configure(EntityTypeBuilder<Segment> builder)
    {
        builder.ToTable("Segments", "master");
        builder.HasKey(s => s.SegmentId);

        builder.Property(s => s.SegmentCode).IsRequired().HasMaxLength(20);
        builder.Property(s => s.SegmentName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Description).HasMaxLength(500);

        builder.HasIndex(s => new { s.TenantId, s.SegmentCode }).IsUnique();

        builder.HasOne(s => s.Tenant).WithMany().HasForeignKey(s => s.TenantId);
    }
}
