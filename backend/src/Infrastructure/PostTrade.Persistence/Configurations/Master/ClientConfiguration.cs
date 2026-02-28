using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Persistence.Configurations.Master;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients", "master");
        builder.HasKey(c => c.ClientId);

        builder.Property(c => c.ClientCode).IsRequired().HasMaxLength(20);
        builder.Property(c => c.ClientName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Phone).HasMaxLength(20);
        builder.Property(c => c.PAN).HasMaxLength(20);
        builder.Property(c => c.Aadhaar).HasMaxLength(12);
        builder.Property(c => c.DPId).HasMaxLength(50);
        builder.Property(c => c.DematAccountNo).HasMaxLength(50);
        builder.Property(c => c.BankAccountNo).HasMaxLength(50);
        builder.Property(c => c.BankName).HasMaxLength(100);
        builder.Property(c => c.BankIFSC).HasMaxLength(20);
        builder.Property(c => c.Address).HasMaxLength(500);
        builder.Property(c => c.StateCode).HasMaxLength(2);
        builder.Property(c => c.StateName).HasMaxLength(100);

        // Onboarding fields
        builder.Property(c => c.Gender).HasMaxLength(20);
        builder.Property(c => c.MaritalStatus).HasMaxLength(30);
        builder.Property(c => c.Occupation).HasMaxLength(100);
        builder.Property(c => c.GrossAnnualIncome).HasMaxLength(50);
        builder.Property(c => c.FatherSpouseName).HasMaxLength(200);
        builder.Property(c => c.MotherName).HasMaxLength(200);
        builder.Property(c => c.AlternateMobile).HasMaxLength(20);
        builder.Property(c => c.City).HasMaxLength(100);
        builder.Property(c => c.PinCode).HasMaxLength(10);
        builder.Property(c => c.CorrespondenceAddress).HasMaxLength(500);

        // Extended onboarding fields
        builder.Property(c => c.HolderType).IsRequired().HasMaxLength(10).HasDefaultValue("Single");
        builder.Property(c => c.CitizenshipStatus).HasMaxLength(50);
        builder.Property(c => c.ResidentialStatus).HasMaxLength(50);
        builder.Property(c => c.IdentityProofType).HasMaxLength(50);
        builder.Property(c => c.IdentityProofNumber).HasMaxLength(100);
        builder.Property(c => c.AccountType).HasMaxLength(20);
        builder.Property(c => c.BranchName).HasMaxLength(100);

        // Non-individual entity fields
        builder.Property(c => c.EntityRegistrationNumber).HasMaxLength(100);
        builder.Property(c => c.ConstitutionType).HasMaxLength(100);
        builder.Property(c => c.GSTNumber).HasMaxLength(20);
        builder.Property(c => c.AnnualTurnover).HasMaxLength(50);
        builder.Property(c => c.KartaName).HasMaxLength(200);
        builder.Property(c => c.KartaPan).HasMaxLength(10);

        builder.HasIndex(c => new { c.TenantId, c.ClientCode }).IsUnique();
        builder.HasIndex(c => new { c.TenantId, c.BrokerId });

        builder.HasOne(c => c.Tenant).WithMany().HasForeignKey(c => c.TenantId);
        builder.HasOne(c => c.Broker).WithMany(b => b.Clients).HasForeignKey(c => c.BrokerId);
        builder.HasOne(c => c.Branch).WithMany(b => b.Clients).HasForeignKey(c => c.BranchId).IsRequired(false);
    }
}
