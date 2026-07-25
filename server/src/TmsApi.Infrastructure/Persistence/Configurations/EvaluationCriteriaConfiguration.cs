namespace TmsApi.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

public class EvaluationCriteriaConfiguration : IEntityTypeConfiguration<EvaluationCriteria>
{
    public void Configure(EntityTypeBuilder<EvaluationCriteria> builder)
    {
        builder.ToTable("EvaluationCriteria");
        builder.HasKey(ec => ec.Id);

        builder.Property(ec => ec.CriteriaName).IsRequired().HasMaxLength(150);
        builder.Property(ec => ec.Description).IsRequired().HasMaxLength(500);

        // Decimal precision for scoring parameters
        builder.Property(ec => ec.WeightPercentage).HasPrecision(5, 2);
        builder.Property(ec => ec.MaxScore).HasPrecision(5, 2);

        builder.HasOne(ec => ec.Tender)
               .WithMany(t => t.Criteria)
               .HasForeignKey(ec => ec.TenderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}