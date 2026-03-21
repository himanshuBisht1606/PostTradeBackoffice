using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Persistence.Configurations.PostTradeProcessing;

public class FoStampDutyConfiguration : IEntityTypeConfiguration<FoStampDuty>
{
    public void Configure(EntityTypeBuilder<FoStampDuty> builder)
    {
        builder.ToTable("FoStampDuties", "post_trade");

        builder.HasKey(s => s.StampDutyRowId);

        builder.Property(s => s.RptHdr).HasMaxLength(5);
        builder.Property(s => s.Sgmt).HasMaxLength(10);
        builder.Property(s => s.Src).HasMaxLength(10);
        builder.Property(s => s.Exchange).HasMaxLength(10);
        builder.Property(s => s.ClrMmbId).HasMaxLength(20);
        builder.Property(s => s.TradngMmbId).HasMaxLength(20);
        builder.Property(s => s.ClntId).HasMaxLength(20);
        builder.Property(s => s.CtrySubDvsn).HasMaxLength(50);
        builder.Property(s => s.TckrSymb).HasMaxLength(50);
        builder.Property(s => s.FinInstrmTp).HasMaxLength(10);
        builder.Property(s => s.Isin).HasMaxLength(20);
        builder.Property(s => s.XpryDt).HasMaxLength(30);
        builder.Property(s => s.StrkPric).HasColumnType("decimal(18,4)");
        builder.Property(s => s.OptnTp).HasMaxLength(5);
        builder.Property(s => s.ClientName).HasMaxLength(200);
        builder.Property(s => s.ClientStateCode).HasMaxLength(5);
        builder.Property(s => s.TtlBuyTrfVal).HasColumnType("decimal(22,4)");
        builder.Property(s => s.TtlSellTrfVal).HasColumnType("decimal(22,4)");
        builder.Property(s => s.BuyDlvryVal).HasColumnType("decimal(22,4)");
        builder.Property(s => s.BuyOthrThanDlvryVal).HasColumnType("decimal(22,4)");
        builder.Property(s => s.BuyStmpDty).HasColumnType("decimal(18,4)");
        builder.Property(s => s.SellStmpDty).HasColumnType("decimal(18,4)");
        builder.Property(s => s.SttlmPric).HasColumnType("decimal(18,4)");
        builder.Property(s => s.BuyDlvryStmpDty).HasColumnType("decimal(18,4)");
        builder.Property(s => s.BuyOthrThanDlvryStmpDty).HasColumnType("decimal(18,4)");
        builder.Property(s => s.StmpDtyAmt).HasColumnType("decimal(18,4)");

        builder.HasIndex(s => new { s.TenantId, s.BatchId, s.ClntId, s.TckrSymb });
    }
}
