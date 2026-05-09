using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Persistence.Configurations.PostTradeProcessing;

public class CmFileImportLogConfiguration : IEntityTypeConfiguration<CmFileImportLog>
{
    public void Configure(EntityTypeBuilder<CmFileImportLog> builder)
    {
        builder.ToTable("CmFileImportLogs", "post_trade");

        builder.HasKey(l => l.LogId);

        builder.Property(l => l.Level).IsRequired().HasMaxLength(10);
        builder.Property(l => l.Message).IsRequired().HasMaxLength(1000);
        builder.Property(l => l.RawData).HasMaxLength(4000);

        builder.HasIndex(l => l.BatchId);
    }
}
