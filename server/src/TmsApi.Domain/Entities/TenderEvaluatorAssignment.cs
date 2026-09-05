namespace TmsApi.Domain.Entities;

public class TenderEvaluatorAssignment
{
    public int Id { get; private set; }
    public int TenderId { get; private set; }
    public int EvaluatorId { get; private set; }
    public DateTime AssignedAt { get; private set; } = DateTime.UtcNow;

    public Tender Tender { get; private set; } = default!;

    private TenderEvaluatorAssignment() { }

    public TenderEvaluatorAssignment(int tenderId, int evaluatorId)
    {
        if (tenderId <= 0) throw new ArgumentOutOfRangeException(nameof(tenderId));
        if (evaluatorId <= 0) throw new ArgumentOutOfRangeException(nameof(evaluatorId));
        TenderId = tenderId;
        EvaluatorId = evaluatorId;
    }
}