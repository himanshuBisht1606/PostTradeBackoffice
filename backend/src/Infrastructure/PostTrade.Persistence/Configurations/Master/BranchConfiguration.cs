using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Persistence.Configurations.Master;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches", "master");
        builder.HasKey(b => b.BranchId);

        builder.Property(b => b.BranchCode).IsRequired().HasMaxLength(20);
        builder.Property(b => b.BranchName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Address).HasMaxLength(500);
        builder.Property(b => b.City).HasMaxLength(100);
        builder.Property(b => b.StateCode).IsRequired().HasMaxLength(2);
        builder.Property(b => b.StateName).IsRequired().HasMaxLength(100);
        builder.Property(b => b.GSTIN).HasMaxLength(15);
        builder.Property(b => b.ContactPerson).HasMaxLength(200);
        builder.Property(b => b.ContactPhone).HasMaxLength(20);
        builder.Property(b => b.ContactEmail).HasMaxLength(200);

        builder.HasIndex(b => new { b.TenantId, b.BranchCode }).IsUnique();

        builder.HasOne(b => b.Tenant).WithMany().HasForeignKey(b => b.TenantId);
    }
}
