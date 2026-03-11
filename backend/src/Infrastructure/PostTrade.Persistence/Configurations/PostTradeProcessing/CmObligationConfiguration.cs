using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Persistence.Configurations.PostTradeProcessing;

public class CmObligationConfiguration : IEntityTypeConfiguration<CmObligation>
{
    public void Configure(EntityTypeBuilder<CmObligation> builder)
    {
        builder.ToTable("CmObligations", "post_trade");

        builder.HasKey(o => o.ObligationRowId);

        builder.Property(o => o.TradngMmbId).HasMaxLength(20);
        builder.Property(o => o.ClntId).HasMaxLength(20);
        builder.Property(o => o.SttlmId).HasMaxLength(20);
        builder.Property(o => o.IsinCode).HasMaxLength(20);
        builder.Property(o => o.ScripNm).HasMaxLength(100);
        builder.Property(o => o.ObligTyp).HasMaxLength(20);
        builder.Property(o => o.ObligStdAmt).HasColumnType("decimal(18,4)");
        builder.Property(o => o.CrObligStdAmt).HasColumnType("decimal(18,4)");
        builder.Property(o => o.DrObligStdAmt).HasColumnType("decimal(18,4)");

        builder.HasIndex(o => new { o.TenantId, o.BatchId, o.ClntId });
    }
}
