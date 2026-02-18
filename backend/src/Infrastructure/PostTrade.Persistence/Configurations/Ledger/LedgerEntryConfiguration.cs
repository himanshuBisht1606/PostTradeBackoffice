using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.Ledger;

namespace PostTrade.Persistence.Configurations.Ledger;

public class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        builder.ToTable("LedgerEntries", "ledger");
        builder.HasKey(l => l.LedgerId);

        builder.Property(l => l.VoucherNo).IsRequired().HasMaxLength(50);
        builder.Property(l => l.ReferenceType).IsRequired().HasMaxLength(50);
        builder.Property(l => l.Narration).HasMaxLength(500);
        builder.Property(l => l.Debit).HasColumnType("decimal(18,4)");
        builder.Property(l => l.Credit).HasColumnType("decimal(18,4)");
        builder.Property(l => l.Balance).HasColumnType("decimal(18,4)");

        builder.HasIndex(l => l.VoucherNo).IsUnique();
        builder.HasIndex(l => new { l.TenantId, l.ClientId, l.PostingDate });
        builder.HasIndex(l => new { l.TenantId, l.ReferenceType, l.ReferenceId });
    }
}
