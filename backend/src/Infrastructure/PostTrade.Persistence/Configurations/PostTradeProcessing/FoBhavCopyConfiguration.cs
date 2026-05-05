using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Persistence.Configurations.PostTradeProcessing;

public class FoBhavCopyConfiguration : IEntityTypeConfiguration<FoBhavCopy>
{
    public void Configure(EntityTypeBuilder<FoBhavCopy> builder)
    {
        builder.ToTable("FoBhavCopies", "post_trade");

        builder.HasKey(b => b.BhavCopyRowId);

        builder.Property(b => b.Exchange).HasMaxLength(10);
        builder.Property(b => b.Sgmt).HasMaxLength(10);
        builder.Property(b => b.Src).HasMaxLength(10);
        builder.Property(b => b.FinInstrmTp).HasMaxLength(10);
        builder.Property(b => b.FinInstrmId).HasMaxLength(50);
        builder.Property(b => b.Isin).HasMaxLength(20);
        builder.Property(b => b.TckrSymb).HasMaxLength(50);
        builder.Property(b => b.SctySrs).HasMaxLength(10);
        builder.Property(b => b.XpryDt).HasMaxLength(30);
        builder.Property(b => b.StrkPric).HasColumnType("decimal(18,4)");
        builder.Property(b => b.OptnTp).HasMaxLength(5);
        builder.Property(b => b.FinInstrmNm).HasMaxLength(100);
        builder.Property(b => b.InstrumentType).HasMaxLength(30);
        builder.Property(b => b.OpnPric).HasColumnType("decimal(18,4)");
        builder.Property(b => b.HghPric).HasColumnType("decimal(18,4)");
        builder.Property(b => b.LwPric).HasColumnType("decimal(18,4)");
        builder.Property(b => b.ClsPric).HasColumnType("decimal(18,4)");
        builder.Property(b => b.LastPric).HasColumnType("decimal(18,4)");
        builder.Property(b => b.PrvsClsgPric).HasColumnType("decimal(18,4)");
        builder.Property(b => b.UndrlygPric).HasColumnType("decimal(18,4)");
        builder.Property(b => b.SttlmPric).HasColumnType("decimal(18,4)");
        builder.Property(b => b.TtlTrfVal).HasColumnType("decimal(22,4)");

        builder.HasIndex(b => new { b.TenantId, b.TradDt, b.Exchange, b.TckrSymb });
    }
}
