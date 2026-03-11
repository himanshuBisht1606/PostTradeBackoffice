using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Persistence.Configurations.Reference;

public class NsdlDpMasterConfiguration : IEntityTypeConfiguration<NsdlDpMaster>
{
    public void Configure(EntityTypeBuilder<NsdlDpMaster> builder)
    {
        builder.ToTable("NsdlDpMaster", "reference");
        builder.HasKey(d => d.DpId);

        builder.Property(d => d.DpCode).IsRequired().HasMaxLength(20);
        builder.Property(d => d.DpName).IsRequired().HasMaxLength(300);
        builder.Property(d => d.SebiRegNo).HasMaxLength(100);
        builder.Property(d => d.City).HasMaxLength(100);
        builder.Property(d => d.State).HasMaxLength(100);
        builder.Property(d => d.PinCode).HasMaxLength(10);
        builder.Property(d => d.Phone).HasMaxLength(50);
        builder.Property(d => d.Email).HasMaxLength(300);
        builder.Property(d => d.MemberStatus).IsRequired().HasMaxLength(10);

        // Global unique — no tenant partitioning
        builder.HasIndex(d => d.DpCode).IsUnique();
    }
}
