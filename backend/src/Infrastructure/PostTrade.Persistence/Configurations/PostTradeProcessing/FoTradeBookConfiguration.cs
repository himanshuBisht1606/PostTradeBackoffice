using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Persistence.Configurations.PostTradeProcessing;

public class FoTradeBookConfiguration : IEntityTypeConfiguration<FoTradeBook>
{
    public void Configure(EntityTypeBuilder<FoTradeBook> builder)
    {
        builder.ToTable("FoTradeBook", "post_trade");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Segment).HasMaxLength(10);
        builder.Property(t => t.Exchange).IsRequired().HasMaxLength(10);
        builder.Property(t => t.UniqueTradeId).IsRequired().HasMaxLength(50);
        builder.Property(t => t.ClearingMemberId).HasMaxLength(20);
        builder.Property(t => t.BrokerId).HasMaxLength(20);
        builder.Property(t => t.BranchCode).HasMaxLength(20);
        builder.Property(t => t.InstrumentId).HasMaxLength(50);
        builder.Property(t => t.Symbol).HasMaxLength(50);
        builder.Property(t => t.InstrumentName).HasMaxLength(100);
        builder.Property(t => t.ContractType).HasMaxLength(30);
        builder.Property(t => t.UnderlyingSymbol).HasMaxLength(50);
        builder.Property(t => t.Isin).HasMaxLength(20);
        builder.Property(t => t.StrikePrice).HasColumnType("decimal(18,4)");
        builder.Property(t => t.OptionType).HasMaxLength(5);
        builder.Property(t => t.ClientType).HasMaxLength(5);
        builder.Property(t => t.ClientCode).HasMaxLength(20);
        builder.Property(t => t.CtclId).HasMaxLength(30);
        builder.Property(t => t.OriginalClientId).HasMaxLength(30);
        builder.Property(t => t.ClientName).HasMaxLength(200);
        builder.Property(t => t.ClientStateCode).HasMaxLength(5);
        builder.Property(t => t.Side).HasMaxLength(5);
        builder.Property(t => t.MarketType).HasMaxLength(30);
        builder.Property(t => t.BookType).HasMaxLength(20);
        builder.Property(t => t.BookTypeName).HasMaxLength(100);
        builder.Property(t => t.Price).HasColumnType("decimal(18,4)");
        builder.Property(t => t.NetPrice).HasColumnType("decimal(18,4)");
        builder.Property(t => t.Brokerage).HasColumnType("decimal(18,4)");
        builder.Property(t => t.TradeValue).HasColumnType("decimal(22,4)");
        builder.Property(t => t.NumberOfLots).HasColumnType("decimal(18,4)");
        builder.Property(t => t.ExerciseAssignmentPrice).HasColumnType("decimal(18,4)");
        builder.Property(t => t.CounterpartyCode).HasMaxLength(20);
        builder.Property(t => t.SettlementType).HasMaxLength(20);
        builder.Property(t => t.SettlementTransactionId).HasMaxLength(50);

        builder.HasIndex(t => new { t.TenantId, t.TradeDate, t.Exchange, t.Symbol });
        builder.HasIndex(t => new { t.TenantId, t.BatchId, t.UniqueTradeId });
        builder.HasIndex(t => new { t.TenantId, t.ClientCode, t.TradeDate });
    }
}
