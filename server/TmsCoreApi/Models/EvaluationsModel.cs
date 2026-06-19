using System;

namespace TmsCoreApi.Models;

public class Evaluation
{
    private decimal _score;

    public required string Id { get; init; }
    public required string BidId { get; init; } // Links to the target Bid being graded
    public required string EvaluatorId { get; init; } // Links to the Evaluation Committee Member
    
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
    public DateTime EvaluatedAt { get; init; } = DateTime.UtcNow;
}