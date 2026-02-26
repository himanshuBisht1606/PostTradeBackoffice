using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Persistence.Configurations.Master;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions", "master");
        builder.HasKey(p => p.PermissionId);

        builder.Property(p => p.PermissionCode).IsRequired().HasMaxLength(100);
        builder.Property(p => p.PermissionName).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.Module).IsRequired().HasMaxLength(100);

        builder.HasIndex(p => p.PermissionCode).IsUnique();

        builder.HasMany(p => p.RolePermissions).WithOne(rp => rp.Permission).HasForeignKey(rp => rp.PermissionId);
    }
}
