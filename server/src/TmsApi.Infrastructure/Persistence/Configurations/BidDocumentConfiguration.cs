namespace TmsApi.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

public class BidDocumentConfiguration : IEntityTypeConfiguration<BidDocument>
{
    public void Configure(EntityTypeBuilder<BidDocument> builder)
    {
        builder.ToTable("BidDocuments");
        builder.HasKey(bd => bd.Id);

        builder.Property(bd => bd.DocumentType).IsRequired().HasMaxLength(100);
        builder.Property(bd => bd.FileName).IsRequired().HasMaxLength(255);
        builder.Property(bd => bd.FilePath).HasMaxLength(500);
        builder.Property(bd => bd.Content).HasColumnType("bytea").IsRequired();
        builder.Property(bd => bd.ContentType).IsRequired().HasMaxLength(150);

        builder.HasOne(bd => bd.Bid)
               .WithMany(b => b.Documents)
               .HasForeignKey(bd => bd.BidId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}