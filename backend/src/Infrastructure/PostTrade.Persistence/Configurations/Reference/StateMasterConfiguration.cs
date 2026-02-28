using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Persistence.Configurations.Reference;

public class StateMasterConfiguration : IEntityTypeConfiguration<StateMaster>
{
    public void Configure(EntityTypeBuilder<StateMaster> builder)
    {
        builder.ToTable("StateMaster", "reference");
        builder.HasKey(s => s.StateId);

        builder.Property(s => s.CountryId).IsRequired().HasMaxLength(5);
        builder.Property(s => s.StateCode).IsRequired().HasMaxLength(10);
        builder.Property(s => s.StateName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.BseName).HasMaxLength(100);

        // Global unique — no tenant partitioning
        builder.HasIndex(s => s.StateCode).IsUnique();
    }
}
