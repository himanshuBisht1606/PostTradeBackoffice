using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Persistence.Configurations.Master;

public class BrokerExchangeMembershipConfiguration : IEntityTypeConfiguration<BrokerExchangeMembership>
{
    public void Configure(EntityTypeBuilder<BrokerExchangeMembership> builder)
    {
        builder.ToTable("BrokerExchangeMemberships", "master");
        builder.HasKey(m => m.BrokerExchangeMembershipId);

        builder.Property(m => m.TradingMemberId).IsRequired().HasMaxLength(20);
        builder.Property(m => m.ClearingMemberId).HasMaxLength(20);

        // A broker can have only one active membership per exchange-segment
        builder.HasIndex(m => new { m.BrokerId, m.ExchangeSegmentId }).IsUnique();

        builder.HasOne(m => m.Broker)
            .WithMany(b => b.ExchangeMemberships)
            .HasForeignKey(m => m.BrokerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.ExchangeSegment)
            .WithMany()
            .HasForeignKey(m => m.ExchangeSegmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
