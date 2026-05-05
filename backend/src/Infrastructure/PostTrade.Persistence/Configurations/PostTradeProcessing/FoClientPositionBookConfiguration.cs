using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Persistence.Configurations.PostTradeProcessing;

public class FoClientPositionBookConfiguration : IEntityTypeConfiguration<FoClientPositionBook>
{
    public void Configure(EntityTypeBuilder<FoClientPositionBook> builder)
    {
        builder.ToTable("FoClientPositionBook", "post_trade");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Exchange).IsRequired().HasMaxLength(10);
        builder.Property(t => t.SegmentIndicator).HasMaxLength(10);
        builder.Property(t => t.ClearingMemberId).HasMaxLength(20);
        builder.Property(t => t.BrokerId).HasMaxLength(20);
        builder.Property(t => t.ClientType).HasMaxLength(5);
        builder.Property(t => t.ClientCode).HasMaxLength(20);
        builder.Property(t => t.ClientName).HasMaxLength(200);
        builder.Property(t => t.ClientStateCode).HasMaxLength(5);
        builder.Property(t => t.Symbol).HasMaxLength(50);
        builder.Property(t => t.ContractName).HasMaxLength(50);
        builder.Property(t => t.ContractType).HasMaxLength(30);
        builder.Property(t => t.Isin).HasMaxLength(20);
        builder.Property(t => t.StrikePrice).HasColumnType("decimal(18,4)");
        builder.Property(t => t.OptionType).HasMaxLength(5);
        builder.Property(t => t.OpenLongValue).HasColumnType("decimal(22,4)");
        builder.Property(t => t.OpenShortValue).HasColumnType("decimal(22,4)");
        builder.Property(t => t.DayBuyValue).HasColumnType("decimal(22,4)");
        builder.Property(t => t.DaySellValue).HasColumnType("decimal(22,4)");
        builder.Property(t => t.PreExerciseLongValue).HasColumnType("decimal(22,4)");
        builder.Property(t => t.PreExerciseShortValue).HasColumnType("decimal(22,4)");
        builder.Property(t => t.PostExerciseLongValue).HasColumnType("decimal(22,4)");
        builder.Property(t => t.PostExerciseShortValue).HasColumnType("decimal(22,4)");
        builder.Property(t => t.SettlementPrice).HasColumnType("decimal(18,4)");
        builder.Property(t => t.ReferenceRate).HasColumnType("decimal(18,4)");
        builder.Property(t => t.PremiumAmount).HasColumnType("decimal(18,4)");
        builder.Property(t => t.NetPremium).HasColumnType("decimal(18,4)");
        builder.Property(t => t.DailyMtmSettlement).HasColumnType("decimal(18,4)");
        builder.Property(t => t.FuturesFinalSettlement).HasColumnType("decimal(18,4)");
        builder.Property(t => t.ExerciseAssignmentValue).HasColumnType("decimal(18,4)");

        builder.HasIndex(t => new { t.TenantId, t.TradeDate, t.Exchange, t.ClientCode });
        builder.HasIndex(t => new { t.TenantId, t.BatchId });
    }
}
