namespace TmsApi.Application.Bids.DTOs;

// Request DTO: Data sent when a supplier submits a bid
public record SubmitBidRequestDto(
    int TenderId,
    int SupplierId,
    decimal FinancialProposalAmount
);

// Request DTO: Data sent when uploading bid supporting documents
public record AddBidDocumentRequestDto(
    int BidId,
    string DocumentType, // e.g., TechnicalProposal, ComplianceCert, FinancialBreakdown
    string FileName,
    string FilePath
);

// Response DTO: Output structure when retrieving bid details
public record BidResponseDto(
    int Id,
    int TenderId,
    int SupplierId,
    decimal FinancialProposalAmount,
    DateTime SubmissionDate,
    string Status,
    List<BidDocumentDto> Documents
);

public record BidDocumentDto(
    int Id,
    string DocumentType,
    string FileName,
    string FilePath,
    DateTime UploadedAt
);