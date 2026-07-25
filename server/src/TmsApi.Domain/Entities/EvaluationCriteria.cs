namespace TmsApi.Domain.Entities;

public class EvaluationCriteria
{
    public int Id { get; private set; }
    public int TenderId { get; private set; }
    public string CriteriaName { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public decimal WeightPercentage { get; private set; }
    public decimal MaxScore { get; private set; }

    // Navigation property
    public Tender Tender { get; private set; } = default!;

    private EvaluationCriteria() { }

    public EvaluationCriteria(int tenderId, string criteriaName, string description, decimal weightPercentage, decimal maxScore)
    {
        if (weightPercentage <= 0 || weightPercentage > 100)
            throw new ArgumentOutOfRangeException(nameof(weightPercentage), "Weight percentage must be between 1 and 100.");

        if (maxScore <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxScore), "Max score must be greater than zero.");

        TenderId = tenderId;
         ArgumentException.ThrowIfNullOrWhiteSpace(criteriaName);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        WeightPercentage = weightPercentage;
        MaxScore = maxScore;
    }
}