using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.Reconciliation;

namespace PostTrade.Persistence.Configurations.Reconciliation;

public class ReconExceptionConfiguration : IEntityTypeConfiguration<ReconException>
{
    public void Configure(EntityTypeBuilder<ReconException> builder)
    {
        builder.ToTable("ReconExceptions", "recon");
        builder.HasKey(e => e.ExceptionId);

        builder.Property(e => e.ExceptionDescription).IsRequired().HasMaxLength(500);
        builder.Property(e => e.ReferenceNo).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Resolution).HasMaxLength(500);
        builder.Property(e => e.Amount).HasColumnType("decimal(18,4)");

        builder.HasIndex(e => new { e.TenantId, e.ReconId });
        builder.HasIndex(e => new { e.TenantId, e.Status });
    }
}
