using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Persistence.Configurations.PostTradeProcessing;

public class CmTradeConfiguration : IEntityTypeConfiguration<CmTrade>
{
    public void Configure(EntityTypeBuilder<CmTrade> builder)
    {
        builder.ToTable("CmTrades", "post_trade");

        builder.HasKey(t => t.TradeRowId);

        builder.Property(t => t.UniqueTradeId).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Sgmt).HasMaxLength(20);
        builder.Property(t => t.Src).HasMaxLength(20);
        builder.Property(t => t.FinInstrmId).HasMaxLength(50);
        builder.Property(t => t.FinInstrmNm).HasMaxLength(100);
        builder.Property(t => t.TradngMmbId).HasMaxLength(20);
        builder.Property(t => t.ClntId).HasMaxLength(20);
        builder.Property(t => t.OrdId).HasMaxLength(50);
        builder.Property(t => t.BuySellInd).HasMaxLength(5);
        builder.Property(t => t.PricePrUnit).HasColumnType("decimal(18,4)");
        builder.Property(t => t.TradVal).HasColumnType("decimal(18,4)");
        builder.Property(t => t.SttlmId).HasMaxLength(20);
        builder.Property(t => t.SttlmTyp).HasMaxLength(20);
        builder.Property(t => t.Exchange).HasMaxLength(10);

        builder.HasIndex(t => new { t.TenantId, t.BatchId, t.UniqueTradeId });
    }
}
