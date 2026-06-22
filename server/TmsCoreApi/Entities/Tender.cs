


namespace TmsCoreApi.Entities;

public enum TenderStatus
{
    Draft,
    Published,
    Closed,
    UnderEvaluation,
    Awarded
}

public class Tender
{
    // Surrogate Primary Key (Auto-incremented by PostgreSQL)
    public int Id { get; set; }

    // Natural Key (e.g., "T-NET2026", unique business identifier)
    public required string ExternalId { get; set; }

    // Title validation using modern C# property backing field syntax
    public required string Title
    {
        get => field;
        set => field = !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ArgumentException("Tender title cannot be empty or whitespace.", nameof(value));
    }

    public required string Description { get; set; }
    public required decimal BudgetLimit { get; set; }
    public required DateTime SubmissionDeadline { get; set; }
    public TenderStatus Status { get; set; } = TenderStatus.Draft;

    // Relational Foreign Keys (Points to User.Id surrogate integer)
    public int CreatedByOfficerId { get; set; }
    public int? WinningBidderId { get; set; }

    // API Legacy / Integration Natural IDs
    public required string CreatedByOfficerExternalId { get; set; }
    public string? WinningBidderExternalId { get; set; }

    // Audit and Tracking Fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // =========================================================================
    // NAVIGATION PROPERTIES (Allows EF to perform optimized SQL joins)
    // =========================================================================
    public User CreatedByOfficer { get; set; } = null!;
    public User? WinningBidder { get; set; }

    // One Tender can receive many Bid submissions
    public ICollection<Bid> Bids { get; set; } = new List<Bid>();
}