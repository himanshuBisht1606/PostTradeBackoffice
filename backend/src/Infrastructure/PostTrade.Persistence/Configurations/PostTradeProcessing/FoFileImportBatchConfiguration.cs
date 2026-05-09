using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Persistence.Configurations.PostTradeProcessing;

public class FoFileImportBatchConfiguration : IEntityTypeConfiguration<FoFileImportBatch>
{
    public void Configure(EntityTypeBuilder<FoFileImportBatch> builder)
    {
        builder.ToTable("FoFileImportBatches", "post_trade");

        builder.HasKey(b => b.BatchId);

        builder.Property(b => b.Exchange).IsRequired().HasMaxLength(10);
        builder.Property(b => b.FileName).IsRequired().HasMaxLength(500);
        builder.Property(b => b.ErrorMessage).HasMaxLength(2000);

        // Unique index: prevent duplicate completed imports for same file type / exchange / date
        builder.HasIndex(b => new { b.TenantId, b.FileType, b.Exchange, b.TradingDate })
               .IsUnique()
               .HasFilter("\"Status\" = 2"); // Completed = 2

        builder.HasMany(b => b.Logs)
               .WithOne(l => l.Batch)
               .HasForeignKey(l => l.BatchId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
