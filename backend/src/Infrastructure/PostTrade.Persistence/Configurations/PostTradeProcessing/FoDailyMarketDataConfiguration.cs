using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Persistence.Configurations.PostTradeProcessing;

public class FoDailyMarketDataConfiguration : IEntityTypeConfiguration<FoDailyMarketData>
{
    public void Configure(EntityTypeBuilder<FoDailyMarketData> builder)
    {
        builder.ToTable("FoDailyMarketData", "post_trade");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Exchange).IsRequired().HasMaxLength(10);
        builder.Property(t => t.InstrumentId).HasMaxLength(50);
        builder.Property(t => t.Symbol).HasMaxLength(50);
        builder.Property(t => t.InstrumentName).HasMaxLength(100);
        builder.Property(t => t.ContractType).HasMaxLength(30);
        builder.Property(t => t.Isin).HasMaxLength(20);
        builder.Property(t => t.StrikePrice).HasColumnType("decimal(18,4)");
        builder.Property(t => t.OptionType).HasMaxLength(5);
        builder.Property(t => t.OpenPrice).HasColumnType("decimal(18,4)");
        builder.Property(t => t.HighPrice).HasColumnType("decimal(18,4)");
        builder.Property(t => t.LowPrice).HasColumnType("decimal(18,4)");
        builder.Property(t => t.ClosePrice).HasColumnType("decimal(18,4)");
        builder.Property(t => t.LastTradedPrice).HasColumnType("decimal(18,4)");
        builder.Property(t => t.PreviousClose).HasColumnType("decimal(18,4)");
        builder.Property(t => t.UnderlyingPrice).HasColumnType("decimal(18,4)");
        builder.Property(t => t.SettlementPrice).HasColumnType("decimal(18,4)");
        builder.Property(t => t.TotalTurnover).HasColumnType("decimal(22,4)");

        builder.HasIndex(t => new { t.TenantId, t.TradeDate, t.Exchange, t.Symbol });
        builder.HasIndex(t => new { t.TenantId, t.BatchId });
    }
}
