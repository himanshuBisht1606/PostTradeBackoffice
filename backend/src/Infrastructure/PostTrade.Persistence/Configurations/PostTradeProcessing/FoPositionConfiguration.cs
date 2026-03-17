using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Persistence.Configurations.PostTradeProcessing;

public class FoPositionConfiguration : IEntityTypeConfiguration<FoPosition>
{
    public void Configure(EntityTypeBuilder<FoPosition> builder)
    {
        builder.ToTable("FoPositions", "post_trade");

        builder.HasKey(p => p.PositionRowId);

        builder.Property(p => p.Sgmt).HasMaxLength(10);
        builder.Property(p => p.Src).HasMaxLength(10);
        builder.Property(p => p.Exchange).HasMaxLength(10);
        builder.Property(p => p.ClrMmbId).HasMaxLength(20);
        builder.Property(p => p.TradngMmbId).HasMaxLength(20);
        builder.Property(p => p.ClntTp).HasMaxLength(5);
        builder.Property(p => p.ClntId).HasMaxLength(20);
        builder.Property(p => p.FinInstrmTp).HasMaxLength(10);
        builder.Property(p => p.Isin).HasMaxLength(20);
        builder.Property(p => p.TckrSymb).HasMaxLength(50);
        builder.Property(p => p.XpryDt).HasMaxLength(30);
        builder.Property(p => p.StrkPric).HasColumnType("decimal(18,4)");
        builder.Property(p => p.OptnTp).HasMaxLength(5);
        builder.Property(p => p.OpngLngVal).HasColumnType("decimal(22,4)");
        builder.Property(p => p.OpngShrtVal).HasColumnType("decimal(22,4)");
        builder.Property(p => p.OpnBuyTradgVal).HasColumnType("decimal(22,4)");
        builder.Property(p => p.OpnSellTradgVal).HasColumnType("decimal(22,4)");
        builder.Property(p => p.PreExrcAssgndLngVal).HasColumnType("decimal(22,4)");
        builder.Property(p => p.PreExrcAssgndShrtVal).HasColumnType("decimal(22,4)");
        builder.Property(p => p.PstExrcAssgndLngVal).HasColumnType("decimal(22,4)");
        builder.Property(p => p.PstExrcAssgndShrtVal).HasColumnType("decimal(22,4)");
        builder.Property(p => p.SttlmPric).HasColumnType("decimal(18,4)");
        builder.Property(p => p.RefRate).HasColumnType("decimal(18,4)");
        builder.Property(p => p.PrmAmt).HasColumnType("decimal(18,4)");
        builder.Property(p => p.DalyMrkToMktSettlmVal).HasColumnType("decimal(18,4)");
        builder.Property(p => p.FutrsFnlSttlmVal).HasColumnType("decimal(18,4)");
        builder.Property(p => p.ExrcAssgndVal).HasColumnType("decimal(18,4)");

        builder.HasIndex(p => new { p.TenantId, p.BatchId, p.ClntId, p.TckrSymb });
    }
}
