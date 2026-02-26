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

        builder.HasIndex(c => new { c.TenantId, c.ClientCode }).IsUnique();
        builder.HasIndex(c => new { c.TenantId, c.BrokerId });

        builder.HasOne(c => c.Tenant).WithMany().HasForeignKey(c => c.TenantId);
        builder.HasOne(c => c.Broker).WithMany(b => b.Clients).HasForeignKey(c => c.BrokerId);
        builder.HasOne(c => c.Branch).WithMany(b => b.Clients).HasForeignKey(c => c.BranchId).IsRequired(false);
    }
}
