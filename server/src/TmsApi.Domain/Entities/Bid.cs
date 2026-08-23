namespace TmsApi.Domain.Entities;

public class Bid
{
    public int Id { get; private set; }
    public int TenderId { get; private set; }
    public int SupplierId { get; private set; }
    
    // Financial primitive for bid price
    public decimal FinancialProposalAmount { get; private set; } 
    public DateTime SubmissionDate { get; private set; }
    public BidStatus Status { get; private set; }
    public bool IsDeleted { get; private set; }

    // Navigation properties
    public Tender Tender { get; private set; } = default!;


    private readonly List<BidDocument> _documents = new();
    public IReadOnlyCollection<BidDocument> Documents => _documents.AsReadOnly();

    private readonly List<BidEvaluation> _evaluations = new();
    public IReadOnlyCollection<BidEvaluation> Evaluations => _evaluations.AsReadOnly();

    private Bid() { }

    public Bid(int tenderId, int supplierId, decimal financialProposalAmount)
    {
        if (tenderId <= 0) throw new ArgumentException("Invalid Tender ID", nameof(tenderId));
        if (supplierId <= 0) throw new ArgumentException("Invalid Supplier ID", nameof(supplierId));
        if (financialProposalAmount <= 0) throw new ArgumentOutOfRangeException(nameof(financialProposalAmount), "Proposal amount must be greater than zero.");

        TenderId = tenderId;
        SupplierId = supplierId;
        FinancialProposalAmount = financialProposalAmount;
        SubmissionDate = DateTime.UtcNow;
        Status = BidStatus.Submitted;
    }

    public void UpdateStatus(BidStatus newStatus)
    {
        Status = newStatus;
    }
}

public enum BidStatus
{
    Submitted = 1,
    UnderReview = 2,
    Awarded = 3,
    Disqualified = 4,
    Rejected = 5
}