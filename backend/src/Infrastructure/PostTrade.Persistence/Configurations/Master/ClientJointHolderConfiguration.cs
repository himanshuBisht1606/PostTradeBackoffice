using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Persistence.Configurations.Master;

public class ClientJointHolderConfiguration : IEntityTypeConfiguration<ClientJointHolder>
{
    public void Configure(EntityTypeBuilder<ClientJointHolder> builder)
    {
        builder.ToTable("ClientJointHolders", "master");
        builder.HasKey(h => h.JointHolderId);

        builder.Property(h => h.Pan).IsRequired().HasMaxLength(10);
        builder.Property(h => h.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(h => h.LastName).IsRequired().HasMaxLength(100);
        builder.Property(h => h.Relationship).IsRequired().HasMaxLength(50);

        builder.HasIndex(h => new { h.ClientId, h.HolderNumber });

        builder.HasOne(h => h.Client)
            .WithMany()
            .HasForeignKey(h => h.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
