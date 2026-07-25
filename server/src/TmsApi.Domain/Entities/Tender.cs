namespace TmsApi.Domain.Entities;

public class Tender
{
    public int Id { get; private set; }
    public string ReferenceNumber { get; private set; } = default!;
    public string Title { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    
    // Financial primitive strictly using base-10 decimal
    public decimal EstimatedBudget { get; private set; } 
    
    public DateTime PublicationDate { get; private set; }
    public DateTime SubmissionDeadline { get; private set; }
    
    public TenderStatus Status { get; private set; }
    public int CreatedByOfficerId { get; private set; }
    
    // Explicitly optional winning bid link
    public int? WinningBidId { get; private set; } 
    public string? CancellationReason { get; private set; }
    public bool IsDeleted { get; private set; }

    // Navigation properties
    public User CreatedByOfficer { get; private set; } = default!;
    public Bid? WinningBid { get; private set; }
    
    private readonly List<TenderDocument> _documents = new();
    public IReadOnlyCollection<TenderDocument> Documents => _documents.AsReadOnly();

    private readonly List<EvaluationCriteria> _criteria = new();
    public IReadOnlyCollection<EvaluationCriteria> Criteria => _criteria.AsReadOnly();

    // Required by ORM for reflection
    private Tender() { } 

    public Tender(
        string referenceNumber, 
        string title, 
        string description, 
        decimal estimatedBudget, 
        DateTime submissionDeadline, 
        int createdByOfficerId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(referenceNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        if (estimatedBudget < 0)
            throw new ArgumentOutOfRangeException(nameof(estimatedBudget), "Estimated budget cannot be negative.");

        if (submissionDeadline <= DateTime.UtcNow)
            throw new ArgumentException("Deadline must be set in the future.", nameof(submissionDeadline));

        ReferenceNumber = referenceNumber;
        Title = title;
        Description = description;
        EstimatedBudget = estimatedBudget;
        PublicationDate = DateTime.UtcNow;
        SubmissionDeadline = submissionDeadline;
        CreatedByOfficerId = createdByOfficerId;
        Status = TenderStatus.Draft;
    }

    public void Publish()
    {
        if (Status != TenderStatus.Draft)
            throw new InvalidOperationException("Only draft tenders can be published.");

        Status = TenderStatus.Published;
    }

    public void CloseTender()
    {
        if (Status != TenderStatus.Published)
            throw new InvalidOperationException("Only published tenders can be closed.");

        Status = TenderStatus.Closed;
    }

    public void AwardToBid(int bidId)
    {
        if (Status != TenderStatus.Closed && Status != TenderStatus.UnderEvaluation)
            throw new InvalidOperationException("Tender must be closed or under evaluation to award.");

        WinningBidId = bidId;
        Status = TenderStatus.Awarded;
    }
}

public enum TenderStatus
{
    Draft = 1,
    Published = 2,
    Closed = 3,
    UnderEvaluation = 4,
    Awarded = 5,
    Cancelled = 6
}