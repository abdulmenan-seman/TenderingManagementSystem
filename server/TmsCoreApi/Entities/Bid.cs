using System;
using System.Collections.Generic;

namespace TmsCoreApi.Entities;

public enum BidStatus
{
    Submitted,
    UnderReview,
    Evaluated,
    Accepted,
    Rejected
}

public class Bid
{
    // Surrogate Primary Key (Auto-incremented by PostgreSQL)
    public int Id { get; set; }

    // Natural Key (e.g., "B-ALPHA-NET", unique business identifier)
    public required string ExternalId { get; set; }

    // Relational Foreign Keys (Points to surrogate integer primary keys)
    public int TenderId { get; set; }
    public int SupplierId { get; set; }

    // API Legacy / Integration Natural IDs
    public required string TenderExternalId { get; set; }
    public required string SupplierExternalId { get; set; }

    public required string TechnicalProposalUrl { get; set; }
    public required decimal FinancialOffer { get; set; }
    public BidStatus Status { get; set; } = BidStatus.Submitted;

    // Audit and Tracking Fields
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // =========================================================================
    // NAVIGATION PROPERTIES (Allows EF to perform optimized SQL joins)
    // =========================================================================
    public Tender Tender { get; set; } = null!;
    public User Supplier { get; set; } = null!;

    // A single bid can receive evaluation reports from different committee members
    public ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
}
