namespace TmsApi.Application.Tenders.DTOs;

// DTO for staging/attaching documents during or before creation
public record CreateTenderDocumentDto(
    string FileName,
    string FilePath
);

// Request DTO: Data sent when creating a new Tender
public record CreateTenderRequestDto(
    string ReferenceNumber,
    string Title,
    string Description,
    decimal EstimatedBudget,
    DateTime SubmissionDeadline,
    List<CreateTenderDocumentDto>? InitialDocuments = null // Optional initial documents
);

// Request DTO: Data sent when attaching a document to an existing Tender
public record UploadTenderDocumentRequestDto(
    int TenderId,
    string FileName,
    string FilePath
);

// Response DTO: Data sent back to clients
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

public record TenderSummaryDto(
    int Id,
    string ReferenceNumber,
    string Title,
    decimal EstimatedBudget,
    DateTime SubmissionDeadline,
    string Status,
    int CreatedByOfficerId,
    List<TenderDocumentDto> Documents
);

public record PaginatedTendersDto(
    List<TenderSummaryDto> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages
);