using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.CorporateActions;

namespace PostTrade.Persistence.Configurations.CorporateActions;

public class CorporateActionConfiguration : IEntityTypeConfiguration<CorporateAction>
{
    public void Configure(EntityTypeBuilder<CorporateAction> builder)
    {
        builder.ToTable("CorporateActions", "corporate");
        builder.HasKey(c => c.CorporateActionId);

        builder.Property(c => c.DividendAmount).HasColumnType("decimal(18,4)");
        builder.Property(c => c.BonusRatio).HasColumnType("decimal(18,6)");
        builder.Property(c => c.SplitRatio).HasColumnType("decimal(18,6)");
        builder.Property(c => c.RightsRatio).HasColumnType("decimal(18,6)");
        builder.Property(c => c.RightsPrice).HasColumnType("decimal(18,4)");

        builder.HasIndex(c => new { c.TenantId, c.InstrumentId, c.ActionType, c.ExDate });
        builder.HasIndex(c => new { c.TenantId, c.ExDate });
    }
}
