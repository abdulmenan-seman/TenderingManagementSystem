namespace TmsApi.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

public class TenderDocumentConfiguration : IEntityTypeConfiguration<TenderDocument>
{
    public void Configure(EntityTypeBuilder<TenderDocument> builder)
    {
        builder.ToTable("TenderDocuments");
        builder.HasKey(td => td.Id);

        builder.Property(td => td.FileName).IsRequired().HasMaxLength(255);
        builder.Property(td => td.FilePath).IsRequired().HasMaxLength(500);

        builder.HasOne(td => td.Tender)
               .WithMany(t => t.Documents)
               .HasForeignKey(td => td.TenderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}