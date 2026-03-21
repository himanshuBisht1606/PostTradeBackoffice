using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Persistence.Configurations.Master;

public class ClientFatcaConfiguration : IEntityTypeConfiguration<ClientFatca>
{
    public void Configure(EntityTypeBuilder<ClientFatca> builder)
    {
        builder.ToTable("ClientFatca", "master");
        builder.HasKey(f => f.ClientFatcaId);

        builder.Property(f => f.TaxCountry).IsRequired().HasMaxLength(100);
        builder.Property(f => f.Tin).HasMaxLength(50);
        builder.Property(f => f.SourceOfWealth).IsRequired().HasMaxLength(100);

        builder.HasIndex(f => f.ClientId);

        builder.HasOne(f => f.Client)
            .WithMany()
            .HasForeignKey(f => f.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
