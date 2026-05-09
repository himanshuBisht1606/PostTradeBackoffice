using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Persistence.Configurations.PostTradeProcessing;

public class CmStampDutyConfiguration : IEntityTypeConfiguration<CmStampDuty>
{
    public void Configure(EntityTypeBuilder<CmStampDuty> builder)
    {
        builder.ToTable("CmStampDuties", "post_trade");

        builder.HasKey(s => s.StampDutyRowId);

        builder.Property(s => s.RptHdr).IsRequired().HasMaxLength(50);
        builder.Property(s => s.TradngMmbId).HasMaxLength(20);
        builder.Property(s => s.ClntId).HasMaxLength(20);
        builder.Property(s => s.Sgmt).HasMaxLength(20);
        builder.Property(s => s.IsinCode).HasMaxLength(20);
        builder.Property(s => s.ScripNm).HasMaxLength(100);
        builder.Property(s => s.BuySellInd).HasMaxLength(5);
        builder.Property(s => s.TradVal).HasColumnType("decimal(18,4)");
        builder.Property(s => s.StmpDtyAmt).HasColumnType("decimal(18,4)");
        builder.Property(s => s.StmpDtyRate).HasColumnType("decimal(10,6)");

        builder.HasIndex(s => new { s.TenantId, s.BatchId });
    }
}
