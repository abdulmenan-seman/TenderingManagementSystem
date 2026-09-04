namespace TmsApi.Domain.Entities;

public class TenderDocument
{
    public int Id { get; private set; }
    public int TenderId { get; private set; }
    public string FileName { get; private set; } = default!;
    public string FilePath { get; private set; } = default!;
    public DateTime UploadedAt { get; private set; } = DateTime.UtcNow;

    public Tender Tender { get; private set; } = default!;

    private TenderDocument() { }

    public TenderDocument(int tenderId, string fileName, string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        TenderId = tenderId;
        FileName = fileName;
        FilePath = filePath;
    }
}