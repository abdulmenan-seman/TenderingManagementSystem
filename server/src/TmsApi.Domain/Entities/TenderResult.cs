namespace TmsApi.Domain.Entities;

public class TenderResult
{
    public int Id { get; private set; }
    public int TenderId { get; private set; }
    public int WinningBidId { get; private set; }
    public int ApprovedByOfficerId { get; private set; }
    
    // Financial primitive strictly using base-10 decimal precision
    public decimal AwardedAmount { get; private set; } 
    public DateTime AwardedAt { get; private set; } = DateTime.UtcNow;
    
    // Explicitly optional evaluation summary / award rationale
    public string? SummaryNotes { get; private set; }

    // Navigation properties
    public Tender Tender { get; private set; } = default!;
    public Bid WinningBid { get; private set; } = default!;
    public User ApprovedByOfficer { get; private set; } = default!;

    // Required by ORM reflection
    private TenderResult() { }

    public TenderResult(
        int tenderId, 
        int winningBidId, 
        int approvedByOfficerId, 
        decimal awardedAmount, 
        string? summaryNotes = null)
    {
        if (tenderId <= 0) 
            throw new ArgumentException("Invalid Tender ID.", nameof(tenderId));
            
        if (winningBidId <= 0) 
            throw new ArgumentException("Invalid Winning Bid ID.", nameof(winningBidId));
            
        if (approvedByOfficerId <= 0) 
            throw new ArgumentException("Invalid Approving Officer ID.", nameof(approvedByOfficerId));

        if (awardedAmount <= 0)
            throw new ArgumentOutOfRangeException(nameof(awardedAmount), "Awarded amount must be greater than zero.");

        TenderId = tenderId;
        WinningBidId = winningBidId;
        ApprovedByOfficerId = approvedByOfficerId;
        AwardedAmount = awardedAmount;
        SummaryNotes = summaryNotes; // Explicitly handles null/optional context
        AwardedAt = DateTime.UtcNow;
    }
}