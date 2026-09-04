namespace TmsApi.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

public class TenderConfiguration : IEntityTypeConfiguration<Tender>
{
    public void Configure(EntityTypeBuilder<Tender> builder)
    {
        builder.ToTable("Tenders");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.ReferenceNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(t => t.ReferenceNumber).IsUnique();

        builder.Property(t => t.Title).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Description).IsRequired().HasMaxLength(2000);
        builder.Property(t => t.EstimatedBudget).HasPrecision(18, 2);

        builder.Property(t => t.Status)
               .HasConversion<int>()
               .IsRequired();

        // Configure backing field access for private read-only list
        builder.Metadata.FindNavigation(nameof(Tender.Documents))!
               .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}