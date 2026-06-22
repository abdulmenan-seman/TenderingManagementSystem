using System;

namespace TmsCoreApi.Models;

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

    public required string Id { get; init; }

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
    public required string CreatedByOfficerId { get; init; } // References User (ProcurementOfficer)
    public string? WinningBidderId { get; set; } // References User (Supplier)
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}