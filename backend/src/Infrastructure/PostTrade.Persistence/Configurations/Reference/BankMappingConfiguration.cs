using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Persistence.Configurations.Reference;

public class BankMappingConfiguration : IEntityTypeConfiguration<BankMapping>
{
    public void Configure(EntityTypeBuilder<BankMapping> builder)
    {
        builder.ToTable("BankMapping", "reference");
        builder.HasKey(m => m.MappingId);

        builder.Property(m => m.BankCode).IsRequired().HasMaxLength(20);
        builder.Property(m => m.IFSCCode).IsRequired().HasMaxLength(11);
        builder.Property(m => m.MICRCode).IsRequired().HasMaxLength(9);

        // Global unique — no tenant partitioning
        builder.HasIndex(m => m.IFSCCode).IsUnique();
    }
}
