using System;

namespace TmsCoreApi.Entities;

public class Evaluation
{
    // Surrogate Primary Key (Auto-incremented by PostgreSQL)
    // Used strictly for efficient database joins and indexing
    public int Id { get; set; }

    // Natural Key / Public Identifier (e.g., "EV-2026-902A")
    // Highly recommended for secure audit logs and safe public API routing
    public required string ExternalId { get; set; }

    // Relational Foreign Keys (Map to surrogate integer IDs of related tables)
    public int BidId { get; set; }
    public int EvaluatorId { get; set; }

    // API Integration Natural IDs (To process incoming JSON payloads from frontend/external systems)
    public required string BidExternalId { get; set; }
    public required string EvaluatorExternalId { get; set; }

    // Private field for backing technical score validation
    private decimal _score;
    public decimal TechnicalScore 
    { 
        get => _score; 
        set
        {
            if (value is < 0m or > 100m)
                throw new ArgumentOutOfRangeException(nameof(value), "Evaluation score must be bounded between 0 and 100.");
            _score = value;
        }
    }
    
    public required string Remarks { get; set; }
    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;

    // =========================================================================
    // NAVIGATION PROPERTIES (Enables EF Core to write optimized SQL joins)
    // =========================================================================
    public Bid Bid { get; set; } = null!;
    public User Evaluator { get; set; } = null!;
}