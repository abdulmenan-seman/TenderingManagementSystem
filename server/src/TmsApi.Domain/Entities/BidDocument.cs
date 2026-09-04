namespace TmsApi.Domain.Entities;

public class BidDocument
{
    public int Id { get; private set; }
    public int BidId { get; private set; }
    public string DocumentType { get; private set; } = default!; // e.g., TechnicalProposal, ComplianceCert, FinancialBreakdown
    public string FileName { get; private set; } = default!;
    public string? FilePath { get; private set; }
    public byte[] Content { get; private set; } = default!;
    public string ContentType { get; private set; } = "application/octet-stream";
    public DateTime UploadedAt { get; private set; } = DateTime.UtcNow;

    // Navigation property
    public Bid Bid { get; private set; } = default!;

    private BidDocument() { }

    public BidDocument(int bidId, string documentType, string fileName, byte[] content, string contentType)
    {
        BidId = bidId;
        if (string.IsNullOrWhiteSpace(documentType)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(documentType));
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(fileName));
        ArgumentNullException.ThrowIfNull(content);
        if (string.IsNullOrWhiteSpace(contentType)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(contentType));

        DocumentType = documentType;
        FileName = fileName;
        Content = content;
        ContentType = contentType;
    }
}