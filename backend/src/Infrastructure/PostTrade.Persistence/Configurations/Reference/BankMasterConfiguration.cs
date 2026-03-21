using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Persistence.Configurations.Reference;

public class BankMasterConfiguration : IEntityTypeConfiguration<BankMaster>
{
    public void Configure(EntityTypeBuilder<BankMaster> builder)
    {
        builder.ToTable("BankMaster", "reference");
        builder.HasKey(b => b.BankId);

        builder.Property(b => b.BankCode).IsRequired().HasMaxLength(20);
        builder.Property(b => b.BankName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.IFSCPrefix).IsRequired().HasMaxLength(20);

        // Global unique — no tenant partitioning
        builder.HasIndex(b => b.BankCode).IsUnique();
    }
}
