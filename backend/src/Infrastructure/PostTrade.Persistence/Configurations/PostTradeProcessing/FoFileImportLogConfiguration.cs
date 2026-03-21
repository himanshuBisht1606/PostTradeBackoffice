using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Persistence.Configurations.PostTradeProcessing;

public class FoFileImportLogConfiguration : IEntityTypeConfiguration<FoFileImportLog>
{
    public void Configure(EntityTypeBuilder<FoFileImportLog> builder)
    {
        builder.ToTable("FoFileImportLogs", "post_trade");

        builder.HasKey(l => l.LogId);

        builder.Property(l => l.Level).IsRequired().HasMaxLength(10);
        builder.Property(l => l.Message).IsRequired().HasMaxLength(2000);
        builder.Property(l => l.RawData).HasMaxLength(1000);

        builder.HasIndex(l => l.BatchId);
    }
}
