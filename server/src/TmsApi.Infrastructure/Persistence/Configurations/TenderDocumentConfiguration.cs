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

public class TenderDocumentConfiguration : IEntityTypeConfiguration<TenderDocument>
{
    public void Configure(EntityTypeBuilder<TenderDocument> builder)
    {
        builder.ToTable("TenderDocuments");
        builder.HasKey(document => document.Id);
        builder.Property(document => document.FileName).IsRequired().HasMaxLength(255);
        builder.Property(document => document.FilePath).HasMaxLength(500);
        builder.Property(document => document.Content).HasColumnType("bytea").IsRequired();
        builder.Property(document => document.ContentType).IsRequired().HasMaxLength(150);

        builder.HasOne(document => document.Tender)
            .WithMany(tender => tender.Documents)
            .HasForeignKey(document => document.TenderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}