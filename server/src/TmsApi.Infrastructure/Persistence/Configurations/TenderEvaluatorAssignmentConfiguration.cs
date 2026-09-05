namespace TmsApi.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

public class TenderEvaluatorAssignmentConfiguration : IEntityTypeConfiguration<TenderEvaluatorAssignment>
{
    public void Configure(EntityTypeBuilder<TenderEvaluatorAssignment> builder)
    {
        builder.ToTable("TenderEvaluatorAssignments");
        builder.HasKey(assignment => assignment.Id);
        builder.HasIndex(assignment => new { assignment.TenderId, assignment.EvaluatorId }).IsUnique();
        builder.HasOne(assignment => assignment.Tender)
            .WithMany()
            .HasForeignKey(assignment => assignment.TenderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}