using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Persistence.Configurations.Reference;

public class PinCodeMasterConfiguration : IEntityTypeConfiguration<PinCodeMaster>
{
    public void Configure(EntityTypeBuilder<PinCodeMaster> builder)
    {
        builder.ToTable("PinCodeMaster", "reference");
        builder.HasKey(p => p.PinCodeId);

        builder.Property(p => p.PinCode).IsRequired().HasMaxLength(10);
        builder.Property(p => p.District).HasMaxLength(100);
        builder.Property(p => p.City).HasMaxLength(100);
        builder.Property(p => p.StateCode).IsRequired().HasMaxLength(10);
        builder.Property(p => p.CountryCode).IsRequired().HasMaxLength(5);
        builder.Property(p => p.McxCode).HasMaxLength(10);

        builder.HasIndex(p => p.PinCode).IsUnique();
    }
}
