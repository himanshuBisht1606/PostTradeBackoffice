using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PostTrade.Persistence.Configurations.Reconciliation;

public class ReconciliationConfiguration : IEntityTypeConfiguration<Domain.Entities.Reconciliation.Reconciliation>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Reconciliation.Reconciliation> builder)
    {
        builder.ToTable("Reconciliations", "recon");
        builder.HasKey(r => r.ReconId);

        builder.Property(r => r.SettlementNo).IsRequired().HasMaxLength(50);
        builder.Property(r => r.Comments).HasMaxLength(500);
        builder.Property(r => r.ResolvedBy).HasMaxLength(100);
        builder.Property(r => r.SystemValue).HasColumnType("decimal(18,4)");
        builder.Property(r => r.ExchangeValue).HasColumnType("decimal(18,4)");
        builder.Property(r => r.Difference).HasColumnType("decimal(18,4)");
        builder.Property(r => r.ToleranceLimit).HasColumnType("decimal(18,4)");

        builder.HasIndex(r => new { r.TenantId, r.SettlementNo, r.ReconType });
        builder.HasIndex(r => new { r.TenantId, r.ReconDate });

        builder.HasMany<Domain.Entities.Reconciliation.ReconException>()
            .WithOne()
            .HasForeignKey(e => e.ReconId);
    }
}
