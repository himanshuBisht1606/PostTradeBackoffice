using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Persistence.Configurations.PostTradeProcessing;

public class FoTradeConfiguration : IEntityTypeConfiguration<FoTrade>
{
    public void Configure(EntityTypeBuilder<FoTrade> builder)
    {
        builder.ToTable("FoTrades", "post_trade");

        builder.HasKey(t => t.TradeRowId);

        builder.Property(t => t.UniqueTradeId).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Sgmt).HasMaxLength(10);
        builder.Property(t => t.Src).HasMaxLength(10);
        builder.Property(t => t.Exchange).HasMaxLength(10);
        builder.Property(t => t.TradngMmbId).HasMaxLength(20);
        builder.Property(t => t.FinInstrmTp).HasMaxLength(10);
        builder.Property(t => t.FinInstrmId).HasMaxLength(50);
        builder.Property(t => t.Isin).HasMaxLength(20);
        builder.Property(t => t.TckrSymb).HasMaxLength(50);
        builder.Property(t => t.XpryDt).HasMaxLength(30);
        builder.Property(t => t.StrkPric).HasColumnType("decimal(18,4)");
        builder.Property(t => t.OptnTp).HasMaxLength(5);
        builder.Property(t => t.FinInstrmNm).HasMaxLength(100);
        builder.Property(t => t.InstrumentType).HasMaxLength(30);
        builder.Property(t => t.UnderlyingSymbol).HasMaxLength(50);
        builder.Property(t => t.ClntTp).HasMaxLength(5);
        builder.Property(t => t.ClntId).HasMaxLength(20);
        builder.Property(t => t.CtclId).HasMaxLength(30);
        builder.Property(t => t.OrgnlCtdnPtcptId).HasMaxLength(30);
        builder.Property(t => t.ClientName).HasMaxLength(200);
        builder.Property(t => t.ClientStateCode).HasMaxLength(5);
        builder.Property(t => t.BuySellInd).HasMaxLength(5);
        builder.Property(t => t.Pric).HasColumnType("decimal(18,4)");
        builder.Property(t => t.SttlmTp).HasMaxLength(20);
        builder.Property(t => t.SctiesSttlmTxId).HasMaxLength(50);
        builder.Property(t => t.TradeValue).HasColumnType("decimal(22,4)");
        builder.Property(t => t.NumLots).HasColumnType("decimal(18,4)");

        builder.HasIndex(t => new { t.TenantId, t.BatchId, t.UniqueTradeId });
        builder.HasIndex(t => new { t.TenantId, t.TradDt, t.Exchange, t.TckrSymb });
    }
}
