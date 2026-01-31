using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.Trading;

namespace PostTrade.Persistence.Configurations.Trading;

public class TradeConfiguration : IEntityTypeConfiguration<Trade>
{
    public void Configure(EntityTypeBuilder<Trade> builder)
    {
        builder.ToTable("Trades", "trading");
        
        builder.HasKey(t => t.TradeId);
        
        builder.Property(t => t.TradeNo).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Price).HasColumnType("decimal(18,4)");
        builder.Property(t => t.TradeValue).HasColumnType("decimal(18,4)");
        builder.Property(t => t.Brokerage).HasColumnType("decimal(18,4)");
        builder.Property(t => t.NetAmount).HasColumnType("decimal(18,4)");
        
        builder.HasIndex(t => new { t.TenantId, t.TradeDate });
        builder.HasIndex(t => new { t.TenantId, t.ClientId, t.InstrumentId });
        builder.HasIndex(t => t.TradeNo).IsUnique();
    }
}
