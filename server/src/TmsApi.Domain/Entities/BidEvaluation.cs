namespace TmsApi.Domain.Entities;

public class BidEvaluation
{
    public int Id { get; private set; }
    public int BidId { get; private set; }
    public int EvaluatorId { get; private set; }
    public int EvaluationCriteriaId { get; private set; }
    
    public decimal AssignedScore { get; private set; }
    public string? Comments { get; private set; }
    public DateTime EvaluatedAt { get; private set; }

    // Navigation properties
    public Bid Bid { get; private set; } = default!;
    public User Evaluator { get; private set; } = default!;
    public EvaluationCriteria EvaluationCriteria { get; private set; } = default!;

    private BidEvaluation() { }

    public BidEvaluation(int bidId, int evaluatorId, int evaluationCriteriaId, decimal assignedScore, string? comments = null)
    {
        if (assignedScore < 0) throw new ArgumentOutOfRangeException(nameof(assignedScore), "Score cannot be negative.");

        BidId = bidId;
        EvaluatorId = evaluatorId;
        EvaluationCriteriaId = evaluationCriteriaId;
        AssignedScore = assignedScore;
        Comments = comments;
        EvaluatedAt = DateTime.UtcNow;
    }
}