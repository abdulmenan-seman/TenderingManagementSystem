namespace TmsApi.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;
public class BidEvaluationConfiguration : IEntityTypeConfiguration<BidEvaluation>
{
    public void Configure(EntityTypeBuilder<BidEvaluation> builder)
    {
        builder.ToTable("BidEvaluations");
        builder.HasKey(be => be.Id);

        builder.Property(be => be.AssignedScore).HasPrecision(5, 2);
        builder.Property(be => be.Comments).HasMaxLength(1000);

        builder.HasOne(be => be.Bid)
               .WithMany(b => b.Evaluations)
               .HasForeignKey(be => be.BidId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}