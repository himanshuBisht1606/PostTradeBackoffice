using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Persistence.Configurations.PostTradeProcessing;

public class CmBhavCopyConfiguration : IEntityTypeConfiguration<CmBhavCopy>
{
    public void Configure(EntityTypeBuilder<CmBhavCopy> builder)
    {
        builder.ToTable("CmBhavCopies", "post_trade");

        builder.HasKey(b => b.BhavCopyRowId);

        builder.Property(b => b.FinInstrmId).IsRequired().HasMaxLength(50);
        builder.Property(b => b.FinInstrmNm).HasMaxLength(100);
        builder.Property(b => b.Isin).HasMaxLength(20);
        builder.Property(b => b.SctySrs).HasMaxLength(10);
        builder.Property(b => b.Exchange).HasMaxLength(10);
        builder.Property(b => b.OpnPric).HasColumnType("decimal(18,4)");
        builder.Property(b => b.HghPric).HasColumnType("decimal(18,4)");
        builder.Property(b => b.LwPric).HasColumnType("decimal(18,4)");
        builder.Property(b => b.ClsPric).HasColumnType("decimal(18,4)");
        builder.Property(b => b.LastPric).HasColumnType("decimal(18,4)");
        builder.Property(b => b.PrvClsgPric).HasColumnType("decimal(18,4)");
        builder.Property(b => b.TtlTrfVal).HasColumnType("decimal(18,2)");
        builder.Property(b => b.MktCpzn).HasColumnType("decimal(22,2)");

        builder.HasIndex(b => new { b.TenantId, b.BatchId, b.FinInstrmId });
    }
}
