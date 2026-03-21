using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Persistence.Configurations.PostTradeProcessing;

public class FoSttLedgerConfiguration : IEntityTypeConfiguration<FoSttLedger>
{
    public void Configure(EntityTypeBuilder<FoSttLedger> builder)
    {
        builder.ToTable("FoSttLedger", "post_trade");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Exchange).IsRequired().HasMaxLength(10);
        builder.Property(t => t.ClearingMemberId).HasMaxLength(20);
        builder.Property(t => t.BrokerId).HasMaxLength(20);
        builder.Property(t => t.ClientCode).HasMaxLength(20);
        builder.Property(t => t.ClientName).HasMaxLength(200);
        builder.Property(t => t.ClientStateCode).HasMaxLength(5);
        builder.Property(t => t.Symbol).HasMaxLength(50);
        builder.Property(t => t.ContractType).HasMaxLength(30);
        builder.Property(t => t.Isin).HasMaxLength(20);
        builder.Property(t => t.StrikePrice).HasColumnType("decimal(18,4)");
        builder.Property(t => t.OptionType).HasMaxLength(5);
        builder.Property(t => t.SettlementPrice).HasColumnType("decimal(18,4)");
        builder.Property(t => t.TotalBuyValue).HasColumnType("decimal(22,4)");
        builder.Property(t => t.TotalSellValue).HasColumnType("decimal(22,4)");
        builder.Property(t => t.TaxableSellFuturesValue).HasColumnType("decimal(22,4)");
        builder.Property(t => t.TaxableSellOptionValue).HasColumnType("decimal(22,4)");
        builder.Property(t => t.OptionExerciseValue).HasColumnType("decimal(22,4)");
        builder.Property(t => t.TaxableExerciseValue).HasColumnType("decimal(22,4)");
        builder.Property(t => t.FuturesStt).HasColumnType("decimal(18,4)");
        builder.Property(t => t.OptionsStt).HasColumnType("decimal(18,4)");
        builder.Property(t => t.FuturesExpiryStt).HasColumnType("decimal(18,4)");
        builder.Property(t => t.OptionsExpiryStt).HasColumnType("decimal(18,4)");
        builder.Property(t => t.TotalStt).HasColumnType("decimal(18,4)");

        builder.HasIndex(t => new { t.TenantId, t.TradeDate, t.Exchange, t.ClientCode });
        builder.HasIndex(t => new { t.TenantId, t.BatchId });
    }
}
