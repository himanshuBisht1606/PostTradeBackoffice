using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Persistence.Configurations.Master;

public class ClientAuthorizedSignatoryConfiguration : IEntityTypeConfiguration<ClientAuthorizedSignatory>
{
    public void Configure(EntityTypeBuilder<ClientAuthorizedSignatory> builder)
    {
        builder.ToTable("ClientAuthorizedSignatories", "master");
        builder.HasKey(s => s.SignatoryId);

        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Designation).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Pan).IsRequired().HasMaxLength(10);
        builder.Property(s => s.Din).HasMaxLength(8);
        builder.Property(s => s.Mobile).HasMaxLength(20);
        builder.Property(s => s.Email).HasMaxLength(200);

        builder.HasIndex(s => s.ClientId);

        builder.HasOne(s => s.Client)
            .WithMany(c => c.AuthorizedSignatories)
            .HasForeignKey(s => s.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
