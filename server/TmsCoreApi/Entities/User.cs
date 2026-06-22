using System;
using System.Collections.Generic;

namespace TmsCoreApi.Entities;

public enum UserRole
{
    Administrator = 0,
    ProcurementOfficer = 1,
    Supplier = 2,
    EvaluationCommitteeMember = 3
}

public class User
{
    // Surrogate Primary Key (auto-incremented by PostgreSQL, 4-byte fast indexing)
    public int Id { get; set; }

    // Natural Key / Identity Key (Auth0 Subject ID, Firebase ID, or Registration code)
    public required string ExternalId { get; set; }

    // Natural Key (Unique index applied during Fluent API registration)
    public required string Email { get; set; }

    public required string FirstName { get; set; }
    
    public required string LastName { get; set; }

    // Computed Property (evaluated in memory, not mapped directly to database columns)
    public string FullName => $"{FirstName} {LastName}".Trim();

    public string? PhoneNumber { get; set; } // Optional field useful for SMS/Email alert triggers

    public required UserRole Role { get; set; }

    // Soft delete flag mapping back to the "IsActive" architectural concept
    public bool IsActive { get; set; } = true;

    // Audit and life-cycle metadata fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }

    // =========================================================================
    // NAVIGATION PROPERTIES (Decoupling actors based on system specs)
    // =========================================================================
    
    // Linked when ProcurementOfficer publishes a Tender
    public ICollection<Tender> CreatedTenders { get; set; } = new List<Tender>();

    // Linked when Supplier submits an electronic proposal
    public ICollection<Bid> SubmittedBids { get; set; } = new List<Bid>();

    // Linked when EvaluationCommitteeMember scores a proposal
    public ICollection<Evaluation> SubmittedEvaluations { get; set; } = new List<Evaluation>();
}
