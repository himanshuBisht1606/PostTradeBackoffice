using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Persistence.Configurations.Master;

public class ClientNomineeConfiguration : IEntityTypeConfiguration<ClientNominee>
{
    public void Configure(EntityTypeBuilder<ClientNominee> builder)
    {
        builder.ToTable("ClientNominees", "master");
        builder.HasKey(n => n.NomineeId);

        builder.Property(n => n.NomineeName).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Relationship).IsRequired().HasMaxLength(50);
        builder.Property(n => n.NomineePAN).HasMaxLength(20);
        builder.Property(n => n.SharePercentage).HasPrecision(5, 2);
        builder.Property(n => n.Mobile).HasMaxLength(20);
        builder.Property(n => n.Email).HasMaxLength(200);
        builder.Property(n => n.Address).HasMaxLength(500);

        builder.HasIndex(n => n.ClientId);

        builder.HasOne(n => n.Client)
            .WithMany(c => c.Nominees)
            .HasForeignKey(n => n.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
