using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Infrastructure.Identity;

namespace TmsApi.Infrastructure.Persistence
{
    public class TmsDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>, ITmsDbContext
    {
        public TmsDbContext(DbContextOptions<TmsDbContext> options) : base(options) { }

        public DbSet<BidderProfile> BidderProfiles => Set<BidderProfile>();
        public DbSet<Tender> Tenders => Set<Tender>();
        public DbSet<TenderEvaluatorAssignment> TenderEvaluatorAssignments => Set<TenderEvaluatorAssignment>();
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
            base.OnModelCreating(modelBuilder); // Must be called first — sets up all Identity tables
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TmsDbContext).Assembly);

            // ── Tender ↔ Bid (one-to-one: WinningBid) ──────────────────────────
            // EF cannot auto-detect the dependent side because both entities have
            // FK-looking properties pointing at each other.
            // We explicitly tell EF: Tender OWNS the FK (WinningBidId → Bid.Id).
            modelBuilder.Entity<Tender>()
                .HasOne(t => t.WinningBid)
                .WithMany()                        // Bid does NOT navigate back via WinningBid
                .HasForeignKey(t => t.WinningBidId)
                .IsRequired(false)                 // A tender may have no winning bid yet
                .OnDelete(DeleteBehavior.SetNull);  // If bid deleted, just clear the FK

            // ── Bid → Tender (many bids belong to one tender) ───────────────────
            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Tender)
                .WithMany()                        // We removed the Bids collection from Tender
                .HasForeignKey(b => b.TenderId)
                .OnDelete(DeleteBehavior.Restrict); // Don't cascade-delete bids if tender deleted

            // ── BidderProfile → ApplicationUser (one-to-one) ────────────────────
            // BidderProfile.UserId references AspNetUsers.Id (int)
            modelBuilder.Entity<BidderProfile>()
                .HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<BidderProfile>(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);  // Delete profile if Identity user is deleted
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
