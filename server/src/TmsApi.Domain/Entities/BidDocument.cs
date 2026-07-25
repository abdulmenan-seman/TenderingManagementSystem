namespace TmsApi.Domain.Entities;

public class BidDocument
{
    public int Id { get; private set; }
    public int BidId { get; private set; }
    public string DocumentType { get; private set; } = default!; // e.g., TechnicalProposal, ComplianceCert, FinancialBreakdown
    public string FileName { get; private set; } = default!;
    public string FilePath { get; private set; } = default!;
    public DateTime UploadedAt { get; private set; } = DateTime.UtcNow;

    // Navigation property
    public Bid Bid { get; private set; } = default!;

    private BidDocument() { }

    public BidDocument(int bidId, string documentType, string fileName, string filePath)
    {
        BidId = bidId;
        if (string.IsNullOrWhiteSpace(documentType)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(documentType));
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(fileName));
        if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(filePath));

        DocumentType = documentType;
        FileName = fileName;
        FilePath = filePath;
    }
}