using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.Settlement;

namespace PostTrade.Persistence.Configurations.Settlement;

public class SettlementBatchConfiguration : IEntityTypeConfiguration<SettlementBatch>
{
    public void Configure(EntityTypeBuilder<SettlementBatch> builder)
    {
        builder.ToTable("SettlementBatches", "settlement");
        builder.HasKey(s => s.BatchId);

        builder.Property(s => s.SettlementNo).IsRequired().HasMaxLength(50);
        builder.Property(s => s.TotalTurnover).HasColumnType("decimal(18,4)");
        builder.Property(s => s.ProcessedBy).HasMaxLength(100);

        builder.HasIndex(s => new { s.TenantId, s.SettlementNo }).IsUnique();
        builder.HasIndex(s => new { s.TenantId, s.TradeDate });

        builder.HasMany(s => s.Obligations).WithOne(o => o.Batch).HasForeignKey(o => o.BatchId);
    }
}
