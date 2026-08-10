namespace TmsApi.Application.Tenders.DTOs;

// Request DTO: Data sent when creating a new Tender
public record CreateTenderRequestDto(
    string ReferenceNumber,
    string Title,
    string Description,
    decimal EstimatedBudget,
    DateTime SubmissionDeadline,
    int CreatedByOfficerId
);

// Request DTO: Data sent when attaching a document to a Tender
public record UploadTenderDocumentRequestDto(
    int TenderId,
    string FileName,
    string FilePath
);

// Response DTO: Clean data sent back to clients viewing a Tender
public record TenderResponseDto(
    int Id,
    string ReferenceNumber,
    string Title,
    string Description,
    decimal EstimatedBudget,
    DateTime PublicationDate,
    DateTime SubmissionDeadline,
    string Status,
    int CreatedByOfficerId,
    List<TenderDocumentDto> Documents
);

public record TenderDocumentDto(
    int Id,
    string FileName,
    string FilePath,
    DateTime UploadedAt
);