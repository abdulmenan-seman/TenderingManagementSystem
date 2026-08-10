namespace TmsApi.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

public class TenderResultConfiguration : IEntityTypeConfiguration<TenderResult>
{
    public void Configure(EntityTypeBuilder<TenderResult> builder)
    {
        builder.ToTable("TenderResults");
        builder.HasKey(tr => tr.Id);

        builder.Property(tr => tr.AwardedAmount).HasPrecision(18, 2);
        builder.Property(tr => tr.SummaryNotes).HasMaxLength(1000);

        builder.HasOne(tr => tr.Tender)
               .WithMany()
               .HasForeignKey(tr => tr.TenderId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(tr => tr.WinningBid)
               .WithMany()
               .HasForeignKey(tr => tr.WinningBidId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}