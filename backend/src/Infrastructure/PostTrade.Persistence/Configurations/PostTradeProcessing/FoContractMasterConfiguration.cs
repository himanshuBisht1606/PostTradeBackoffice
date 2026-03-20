using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Persistence.Configurations.PostTradeProcessing;

public class FoContractMasterConfiguration : IEntityTypeConfiguration<FoContractMaster>
{
    public void Configure(EntityTypeBuilder<FoContractMaster> builder)
    {
        builder.ToTable("FoContractMasters", "post_trade");

        builder.HasKey(c => c.ContractRowId);

        builder.Property(c => c.Exchange).IsRequired().HasMaxLength(10);
        builder.Property(c => c.FinInstrmId).IsRequired().HasMaxLength(50);
        builder.Property(c => c.UndrlygFinInstrmId).HasMaxLength(50);
        builder.Property(c => c.FinInstrmNm).HasMaxLength(100);
        builder.Property(c => c.TckrSymb).IsRequired().HasMaxLength(50);
        builder.Property(c => c.XpryDt).HasMaxLength(30);
        builder.Property(c => c.StrkPric).HasColumnType("decimal(18,4)");
        builder.Property(c => c.OptnTp).HasMaxLength(5);
        builder.Property(c => c.FinInstrmTp).HasMaxLength(10);
        builder.Property(c => c.SttlmMtd).HasMaxLength(10);
        builder.Property(c => c.StockNm).HasMaxLength(100);
        builder.Property(c => c.TickSize).HasColumnType("decimal(18,4)");
        builder.Property(c => c.BasePric).HasColumnType("decimal(18,4)");
        builder.Property(c => c.MktTpAndId).HasMaxLength(20);
        builder.Property(c => c.OptnExrcStyle).HasMaxLength(5);
        builder.Property(c => c.Isin).HasMaxLength(20);

        builder.Property(c => c.RegisteredInstrumentId).IsRequired(false);

        builder.HasIndex(c => new { c.TenantId, c.Exchange, c.TradingDate, c.TckrSymb });
    }
}
