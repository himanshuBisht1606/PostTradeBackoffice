using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Persistence.Configurations.PostTradeProcessing;

public class FoContractConfiguration : IEntityTypeConfiguration<FoContract>
{
    public void Configure(EntityTypeBuilder<FoContract> builder)
    {
        builder.ToTable("FoContracts", "post_trade");
        builder.HasKey(c => c.ContractId);

        builder.Property(c => c.Exchange).IsRequired().HasMaxLength(10);
        builder.Property(c => c.InstrumentType).IsRequired().HasMaxLength(10);
        builder.Property(c => c.Symbol).IsRequired().HasMaxLength(50);
        builder.Property(c => c.ContractName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.OptionType).HasMaxLength(5);
        builder.Property(c => c.StrikePrice).HasColumnType("decimal(18,4)");
        builder.Property(c => c.FMultiplier).HasColumnType("decimal(18,6)").HasDefaultValue(1m);
        builder.Property(c => c.TickSize).HasColumnType("decimal(18,4)");
        builder.Property(c => c.UnderlyingSymbol).HasMaxLength(50);
        builder.Property(c => c.FinInstrmId).HasMaxLength(50);
        builder.Property(c => c.Isin).HasMaxLength(20);
        builder.Property(c => c.SttlmMtd).HasMaxLength(10);

        // Primary lookup: by FinInstrmId (exchange token) during trade import enrichment
        builder.HasIndex(c => new { c.TenantId, c.Exchange, c.TradingDate, c.FinInstrmId });
        // Secondary lookup: by Symbol + ExpiryDate + OptionType (for charge/contract validation)
        builder.HasIndex(c => new { c.TenantId, c.Exchange, c.TradingDate, c.Symbol, c.ExpiryDate });
        // Registration back-link
        builder.Property(c => c.RegisteredInstrumentId);
    }
}
