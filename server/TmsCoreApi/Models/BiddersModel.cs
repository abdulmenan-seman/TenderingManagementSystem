using System;

namespace TmsCoreApi.Models;

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
    public required string Id { get; init; }
    public required string TenderId { get; init; } // Links directly to the source Tender
    public required string SupplierId { get; init; } // Links to the bidding Supplier
    public required string TechnicalProposalUrl { get; set; } // Path reference to uploaded documents
    public required decimal FinancialOffer { get; set; }
    public BidStatus Status { get; set; } = BidStatus.Submitted;
    public DateTime SubmittedAt { get; init; } = DateTime.UtcNow;
}