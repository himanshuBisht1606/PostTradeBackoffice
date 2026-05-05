using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Persistence.Configurations.PostTradeProcessing;

public class FoFinanceLedgerConfiguration : IEntityTypeConfiguration<FoFinanceLedger>
{
    public void Configure(EntityTypeBuilder<FoFinanceLedger> builder)
    {
        builder.ToTable("FoFinanceLedger", "post_trade");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Exchange).IsRequired().HasMaxLength(10);
        builder.Property(t => t.ClearingMemberId).HasMaxLength(20);
        builder.Property(t => t.BrokerId).HasMaxLength(20);
        builder.Property(t => t.ClientCode).IsRequired().HasMaxLength(20);
        builder.Property(t => t.ClientName).HasMaxLength(200);

        builder.Property(t => t.BuyTurnover).HasColumnType("decimal(22,4)");
        builder.Property(t => t.SellTurnover).HasColumnType("decimal(22,4)");
        builder.Property(t => t.TotalTurnover).HasColumnType("decimal(22,4)");

        builder.Property(t => t.TotalStt).HasColumnType("decimal(18,4)");
        builder.Property(t => t.TotalStampDuty).HasColumnType("decimal(18,4)");
        builder.Property(t => t.Brokerage).HasColumnType("decimal(18,4)");
        builder.Property(t => t.ExchangeTransactionCharges).HasColumnType("decimal(18,4)");
        builder.Property(t => t.SebiCharges).HasColumnType("decimal(18,4)");
        builder.Property(t => t.Ipft).HasColumnType("decimal(18,4)");
        builder.Property(t => t.GstOnCharges).HasColumnType("decimal(18,4)");
        builder.Property(t => t.TotalCharges).HasColumnType("decimal(18,4)");

        builder.Property(t => t.DailyMtmSettlement).HasColumnType("decimal(18,4)");
        builder.Property(t => t.NetPremium).HasColumnType("decimal(18,4)");
        builder.Property(t => t.FinalSettlement).HasColumnType("decimal(18,4)");
        builder.Property(t => t.ExerciseAssignmentValue).HasColumnType("decimal(18,4)");
        builder.Property(t => t.NetAmount).HasColumnType("decimal(18,4)");

        // One row per client per trade date per exchange
        builder.HasIndex(t => new { t.TenantId, t.TradeDate, t.Exchange, t.ClientCode }).IsUnique();
        builder.HasIndex(t => new { t.TenantId, t.TradeDate, t.Exchange });
        builder.HasIndex(t => new { t.TenantId, t.ClientId });
    }
}
