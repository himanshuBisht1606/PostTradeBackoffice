using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Persistence.Configurations.Master;

public class BrokerConfiguration : IEntityTypeConfiguration<Broker>
{
    public void Configure(EntityTypeBuilder<Broker> builder)
    {
        builder.ToTable("Brokers", "master");
        builder.HasKey(b => b.BrokerId);

        // Identity
        builder.Property(b => b.BrokerCode).IsRequired().HasMaxLength(20);
        builder.Property(b => b.BrokerName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.LogoUrl).HasMaxLength(500);
        builder.Property(b => b.Website).HasMaxLength(200);

        // Company / Corporate
        builder.Property(b => b.CIN).HasMaxLength(21);
        builder.Property(b => b.TAN).HasMaxLength(10);
        builder.Property(b => b.PAN).HasMaxLength(10);
        builder.Property(b => b.GST).HasMaxLength(15);

        // Contact
        builder.Property(b => b.ContactEmail).IsRequired().HasMaxLength(200);
        builder.Property(b => b.ContactPhone).IsRequired().HasMaxLength(20);

        // Registered Address
        builder.Property(b => b.RegisteredAddressLine1).HasMaxLength(200);
        builder.Property(b => b.RegisteredAddressLine2).HasMaxLength(200);
        builder.Property(b => b.RegisteredCity).HasMaxLength(100);
        builder.Property(b => b.RegisteredState).HasMaxLength(100);
        builder.Property(b => b.RegisteredPinCode).HasMaxLength(10);
        builder.Property(b => b.RegisteredCountry).HasMaxLength(50).HasDefaultValue("India");

        // Correspondence Address
        builder.Property(b => b.CorrespondenceAddressLine1).HasMaxLength(200);
        builder.Property(b => b.CorrespondenceAddressLine2).HasMaxLength(200);
        builder.Property(b => b.CorrespondenceCity).HasMaxLength(100);
        builder.Property(b => b.CorrespondenceState).HasMaxLength(100);
        builder.Property(b => b.CorrespondencePinCode).HasMaxLength(10);

        // SEBI Registration
        builder.Property(b => b.SEBIRegistrationNo).HasMaxLength(50);

        // Compliance Officers
        builder.Property(b => b.ComplianceOfficerName).HasMaxLength(200);
        builder.Property(b => b.ComplianceOfficerEmail).HasMaxLength(200);
        builder.Property(b => b.ComplianceOfficerPhone).HasMaxLength(20);
        builder.Property(b => b.PrincipalOfficerName).HasMaxLength(200);
        builder.Property(b => b.PrincipalOfficerEmail).HasMaxLength(200);
        builder.Property(b => b.PrincipalOfficerPhone).HasMaxLength(20);

        // Settlement Bank
        builder.Property(b => b.SettlementBankName).HasMaxLength(200);
        builder.Property(b => b.SettlementBankAccountNo).HasMaxLength(30);
        builder.Property(b => b.SettlementBankIfsc).HasMaxLength(11);
        builder.Property(b => b.SettlementBankBranch).HasMaxLength(200);

        builder.HasIndex(b => new { b.TenantId, b.BrokerCode }).IsUnique();

        builder.HasOne(b => b.Tenant).WithMany(t => t.Brokers).HasForeignKey(b => b.TenantId);
        builder.HasMany(b => b.Clients).WithOne(c => c.Broker).HasForeignKey(c => c.BrokerId);
        builder.HasMany(b => b.ExchangeMemberships).WithOne(m => m.Broker).HasForeignKey(m => m.BrokerId);
    }
}
