using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.Settlement;

namespace PostTrade.Persistence.Configurations.Settlement;

public class SettlementObligationConfiguration : IEntityTypeConfiguration<SettlementObligation>
{
    public void Configure(EntityTypeBuilder<SettlementObligation> builder)
    {
        builder.ToTable("SettlementObligations", "settlement");
        builder.HasKey(o => o.ObligationId);

        builder.Property(o => o.SettlementNo).IsRequired().HasMaxLength(50);
        builder.Property(o => o.FundsPayIn).HasColumnType("decimal(18,4)");
        builder.Property(o => o.FundsPayOut).HasColumnType("decimal(18,4)");
        builder.Property(o => o.NetFundsObligation).HasColumnType("decimal(18,4)");

        builder.HasIndex(o => new { o.TenantId, o.SettlementNo, o.ClientId }).IsUnique();
        builder.HasIndex(o => new { o.TenantId, o.BatchId });

        builder.HasOne(o => o.Batch).WithMany(b => b.Obligations).HasForeignKey(o => o.BatchId);
    }
}
