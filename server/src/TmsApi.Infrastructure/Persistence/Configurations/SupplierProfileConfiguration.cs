namespace TmsApi.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

public class SupplierProfileConfiguration : IEntityTypeConfiguration<SupplierProfile>
{
    public void Configure(EntityTypeBuilder<SupplierProfile> builder)
    {
        builder.ToTable("SupplierProfiles");
        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.CompanyName).IsRequired().HasMaxLength(200);
        builder.Property(sp => sp.TaxIdNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(sp => sp.TaxIdNumber).IsUnique();

        builder.Property(sp => sp.BusinessLicenseNumber).IsRequired().HasMaxLength(100);
        builder.Property(sp => sp.Address).IsRequired().HasMaxLength(300);
        builder.Property(sp => sp.PhoneNumber).IsRequired().HasMaxLength(20);

        builder.HasOne(sp => sp.User)
               .WithOne()
               .HasForeignKey<SupplierProfile>(sp => sp.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}