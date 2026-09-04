namespace TmsApi.Domain.Entities;

public class TenderDocument
{
    public int Id { get; private set; }
    public int TenderId { get; private set; }
    public string FileName { get; private set; } = default!;
    public byte[] Content { get; private set; } = default!;
    public string ContentType { get; private set; } = "application/octet-stream";
    public DateTime UploadedAt { get; private set; } = DateTime.UtcNow;

    public Tender Tender { get; private set; } = default!;

    private TenderDocument() { }

    public TenderDocument(int tenderId, string fileName, byte[] content, string contentType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        TenderId = tenderId;
        FileName = fileName;
        Content = content;
        ContentType = contentType;
    }
}