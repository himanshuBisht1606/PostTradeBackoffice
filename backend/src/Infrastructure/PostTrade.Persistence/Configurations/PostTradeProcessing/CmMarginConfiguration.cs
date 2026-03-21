using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Persistence.Configurations.PostTradeProcessing;

public class CmMarginConfiguration : IEntityTypeConfiguration<CmMargin>
{
    public void Configure(EntityTypeBuilder<CmMargin> builder)
    {
        builder.ToTable("CmMargins", "post_trade");

        builder.HasKey(m => m.MarginRowId);

        builder.Property(m => m.TradngMmbId).HasMaxLength(20);
        builder.Property(m => m.ClntId).HasMaxLength(20);
        builder.Property(m => m.Sgmt).HasMaxLength(20);
        builder.Property(m => m.IsinCode).HasMaxLength(20);
        builder.Property(m => m.ScripNm).HasMaxLength(100);
        builder.Property(m => m.MtmMrgnAmt).HasColumnType("decimal(18,4)");
        builder.Property(m => m.VrMrgnAmt).HasColumnType("decimal(18,4)");
        builder.Property(m => m.ExpsrMrgnAmt).HasColumnType("decimal(18,4)");
        builder.Property(m => m.AddhcMrgnAmt).HasColumnType("decimal(18,4)");
        builder.Property(m => m.CrystldLssAmt).HasColumnType("decimal(18,4)");
        builder.Property(m => m.TtlMrgnAmt).HasColumnType("decimal(18,4)");

        builder.HasIndex(m => new { m.TenantId, m.BatchId, m.ClntId });
    }
}
