using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Persistence.Configurations.PostTradeProcessing;

public class CmSettlementMasterConfiguration : IEntityTypeConfiguration<CmSettlementMaster>
{
    public void Configure(EntityTypeBuilder<CmSettlementMaster> builder)
    {
        builder.ToTable("CmSettlementMasters", "post_trade");

        builder.HasKey(s => s.CmSettlementMasterId);

        builder.Property(s => s.Exchange).IsRequired().HasMaxLength(10);
        builder.Property(s => s.SettlementNo).IsRequired().HasMaxLength(20);
        builder.Property(s => s.SettlementType).IsRequired().HasMaxLength(20);

        builder.HasIndex(s => new { s.TenantId, s.Exchange, s.TradingDate, s.SettlementNo }).IsUnique();
    }
}
