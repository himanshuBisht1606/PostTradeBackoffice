using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Persistence.Configurations.PostTradeProcessing;

public class FoStampDutyLedgerConfiguration : IEntityTypeConfiguration<FoStampDutyLedger>
{
    public void Configure(EntityTypeBuilder<FoStampDutyLedger> builder)
    {
        builder.ToTable("FoStampDutyLedger", "post_trade");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Exchange).IsRequired().HasMaxLength(10);
        builder.Property(t => t.ClearingMemberId).HasMaxLength(20);
        builder.Property(t => t.BrokerId).HasMaxLength(20);
        builder.Property(t => t.ClientCode).HasMaxLength(20);
        builder.Property(t => t.ClientName).HasMaxLength(200);
        builder.Property(t => t.ClientStateCode).HasMaxLength(5);
        builder.Property(t => t.StateCode).HasMaxLength(50);
        builder.Property(t => t.StateName).HasMaxLength(100);
        builder.Property(t => t.Symbol).HasMaxLength(50);
        builder.Property(t => t.ContractType).HasMaxLength(30);
        builder.Property(t => t.Isin).HasMaxLength(20);
        builder.Property(t => t.StrikePrice).HasColumnType("decimal(18,4)");
        builder.Property(t => t.OptionType).HasMaxLength(5);
        builder.Property(t => t.SettlementPrice).HasColumnType("decimal(18,4)");
        builder.Property(t => t.TotalBuyValue).HasColumnType("decimal(22,4)");
        builder.Property(t => t.TotalSellValue).HasColumnType("decimal(22,4)");
        builder.Property(t => t.DeliveryBuyValue).HasColumnType("decimal(22,4)");
        builder.Property(t => t.NonDeliveryBuyValue).HasColumnType("decimal(22,4)");
        builder.Property(t => t.BuyStampDuty).HasColumnType("decimal(18,4)");
        builder.Property(t => t.SellStampDuty).HasColumnType("decimal(18,4)");
        builder.Property(t => t.FuturesStampDuty).HasColumnType("decimal(18,4)");
        builder.Property(t => t.OptionsStampDuty).HasColumnType("decimal(18,4)");
        builder.Property(t => t.FuturesExpiryStampDuty).HasColumnType("decimal(18,4)");
        builder.Property(t => t.OptionsExpiryStampDuty).HasColumnType("decimal(18,4)");
        builder.Property(t => t.DeliveryBuyStampDuty).HasColumnType("decimal(18,4)");
        builder.Property(t => t.NonDeliveryBuyStampDuty).HasColumnType("decimal(18,4)");
        builder.Property(t => t.TotalStampDuty).HasColumnType("decimal(18,4)");

        builder.HasIndex(t => new { t.TenantId, t.TradeDate, t.Exchange, t.ClientCode });
        builder.HasIndex(t => new { t.TenantId, t.BatchId });
    }
}
