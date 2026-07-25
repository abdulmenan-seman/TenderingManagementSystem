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

        // Financial Primitive: Explicit Base-10 Decimal Precision (18 digits, 2 decimal places)
        builder.Property(t => t.EstimatedBudget).HasPrecision(18, 2);

        // Shadow Column for System Auditing
        builder.Property<DateTime>("LastUpdated");

        // Global Soft Delete Query Filter
        builder.HasQueryFilter(t => !t.IsDeleted);

        // Relationships
        builder.HasOne(t => t.CreatedByOfficer)
               .WithMany()
               .HasForeignKey(t => t.CreatedByOfficerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.WinningBid)
               .WithMany()
               .HasForeignKey(t => t.WinningBidId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}