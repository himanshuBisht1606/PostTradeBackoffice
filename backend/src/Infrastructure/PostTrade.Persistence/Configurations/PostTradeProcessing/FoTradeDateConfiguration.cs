using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Persistence.Configurations.PostTradeProcessing;

public class FoTradeDateConfiguration : IEntityTypeConfiguration<FoTradeDate>
{
    public void Configure(EntityTypeBuilder<FoTradeDate> builder)
    {
        builder.ToTable("FoTradeDate", "post_trade");
        builder.HasKey(t => t.Id);

        // TrnSlNo is generated from the shared fo_trn_slno_seq sequence on insert.
        // The sequence is defined in the migration (post_trade.fo_trn_slno_seq).
        builder.Property(t => t.TrnSlNo)
            .HasDefaultValueSql("nextval('post_trade.fo_trn_slno_seq')");

        builder.Property(t => t.Exchange).IsRequired().HasMaxLength(10);
        builder.Property(t => t.GlobalExchange).IsRequired().HasMaxLength(10);
        builder.Property(t => t.Segment).HasMaxLength(10);
        builder.Property(t => t.UniqueTradeId).IsRequired().HasMaxLength(50);
        builder.Property(t => t.TrdType).IsRequired().HasMaxLength(5);
        builder.Property(t => t.ClearingMemberId).HasMaxLength(20);
        builder.Property(t => t.TradingMemberId).HasMaxLength(20);
        builder.Property(t => t.InstrumentType).HasMaxLength(10);
        builder.Property(t => t.Symbol).HasMaxLength(50);
        builder.Property(t => t.ContractName).HasMaxLength(100);
        builder.Property(t => t.InstrumentId).HasMaxLength(50);
        builder.Property(t => t.OptionType).HasMaxLength(5);
        builder.Property(t => t.UnderlyingSymbol).HasMaxLength(50);
        builder.Property(t => t.Isin).HasMaxLength(20);
        builder.Property(t => t.StrikePrice).HasColumnType("decimal(18,4)");
        builder.Property(t => t.FMultiplier).HasColumnType("decimal(18,6)").HasDefaultValue(1m);
        builder.Property(t => t.ClientCode).HasMaxLength(20);
        builder.Property(t => t.OriginalClientCode).HasMaxLength(30);
        builder.Property(t => t.ClientType).HasMaxLength(5);
        builder.Property(t => t.CtclId).HasMaxLength(30);
        builder.Property(t => t.ClientName).HasMaxLength(200);
        builder.Property(t => t.ClientStateCode).HasMaxLength(5);
        builder.Property(t => t.Side).HasMaxLength(5);
        builder.Property(t => t.NumberOfLots).HasColumnType("decimal(18,4)");
        builder.Property(t => t.Price).HasColumnType("decimal(18,4)");
        builder.Property(t => t.NetPrice).HasColumnType("decimal(18,4)");
        builder.Property(t => t.TradeValue).HasColumnType("decimal(22,4)");
        builder.Property(t => t.TradeStatus).HasMaxLength(10);
        builder.Property(t => t.OrderRef).HasMaxLength(50);
        builder.Property(t => t.MarketType).HasMaxLength(30);
        builder.Property(t => t.BookType).HasMaxLength(20);
        builder.Property(t => t.BookTypeName).HasMaxLength(100);
        builder.Property(t => t.SettlementType).HasMaxLength(20);
        builder.Property(t => t.SettlementTransactionId).HasMaxLength(50);
        builder.Property(t => t.CounterpartyCode).HasMaxLength(20);
        builder.Property(t => t.Remarks).HasMaxLength(500);
        builder.Property(t => t.FileName).HasMaxLength(500);

        // Dedup guard: same exchange trade leg for the same client+side can only appear once per date.
        // UniqueTradeId alone is NOT unique — intra-broker trades have the same UniqueTradeId
        // for both the buy leg (client A) and the sell leg (client B).
        builder.HasIndex(t => new { t.TenantId, t.TradeDate, t.Exchange, t.UniqueTradeId, t.ClientCode, t.Side }).IsUnique();
        // Batch-level delete on re-import
        builder.HasIndex(t => new { t.TenantId, t.BatchId });
        // GlobalExchange + date for bill generation grouping
        builder.HasIndex(t => new { t.TenantId, t.GlobalExchange, t.TradeDate, t.ClientCode });

        builder.HasOne(t => t.Batch).WithMany().HasForeignKey(t => t.BatchId);
    }
}
