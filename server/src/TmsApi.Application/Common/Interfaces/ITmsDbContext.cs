namespace TmsApi.Application.Common.Interfaces;

using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;

public interface ITmsDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<SupplierProfile> SupplierProfiles { get; }
    DbSet<Tender> Tenders { get; }
    DbSet<TenderDocument> TenderDocuments { get; }
    DbSet<EvaluationCriteria> EvaluationCriteria { get; }
    DbSet<Bid> Bids { get; }
    DbSet<BidDocument> BidDocuments { get; }
    DbSet<BidEvaluation> BidEvaluations { get; }
    DbSet<TenderResult> TenderResults { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}