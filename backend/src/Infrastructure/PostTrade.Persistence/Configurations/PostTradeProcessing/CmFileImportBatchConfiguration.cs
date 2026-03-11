using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Persistence.Configurations.PostTradeProcessing;

public class CmFileImportBatchConfiguration : IEntityTypeConfiguration<CmFileImportBatch>
{
    public void Configure(EntityTypeBuilder<CmFileImportBatch> builder)
    {
        builder.ToTable("CmFileImportBatches", "post_trade");

        builder.HasKey(b => b.BatchId);

        builder.Property(b => b.Exchange).IsRequired().HasMaxLength(10);
        builder.Property(b => b.FileName).IsRequired().HasMaxLength(500);
        builder.Property(b => b.ErrorMessage).HasMaxLength(2000);

        // Unique index for duplicate detection
        builder.HasIndex(b => new { b.TenantId, b.FileType, b.Exchange, b.TradingDate })
               .IsUnique()
               .HasFilter("\"Status\" = 2"); // Completed = 2

        builder.HasMany(b => b.Logs)
               .WithOne(l => l.Batch)
               .HasForeignKey(l => l.BatchId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
