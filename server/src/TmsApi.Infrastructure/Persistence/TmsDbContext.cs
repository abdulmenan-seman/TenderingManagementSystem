using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;
using TmsApi.Application.Common.Interfaces;

namespace TmsApi.Infrastructure.Persistence
{
    public class TmsDbContext : DbContext, ITmsDbContext
    {
        public TmsDbContext(DbContextOptions<TmsDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<SupplierProfile> SupplierProfiles => Set<SupplierProfile>();
        public DbSet<Tender> Tenders => Set<Tender>();
        public DbSet<TenderDocument> TenderDocuments => Set<TenderDocument>();
        public DbSet<EvaluationCriteria> EvaluationCriteria => Set<EvaluationCriteria>();
        public DbSet<Bid> Bids => Set<Bid>();
        public DbSet<BidDocument> BidDocuments => Set<BidDocument>();
        public DbSet<BidEvaluation> BidEvaluations => Set<BidEvaluation>();
        public DbSet<TenderResult> TenderResults => Set<TenderResult>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TmsDbContext).Assembly);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State is EntityState.Added or EntityState.Modified)
                {
                    if (entry.Properties.Any(p => p.Metadata.Name == "LastUpdated"))
                    {
                        entry.Property("LastUpdated").CurrentValue = DateTime.UtcNow;
                    }
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
