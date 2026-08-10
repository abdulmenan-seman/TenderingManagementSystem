namespace TmsApi.Application.Evaluations.DTOs;

// Incoming request to score a bid against a specific criterion
public record SubmitBidEvaluationRequestDto(
    int BidId,
    int CriteriaId,
    int EvaluatorId,
    decimal Score,
    string Remarks
);

// Incoming request to finalize award for a tender
public record FinalizeTenderResultRequestDto(
    int TenderId,
    int WinningBidId,
    int AwardedByOfficerId,
    string Remarks
);

// Response DTO for viewing evaluation details
public record BidEvaluationResponseDto(
    int Id,
    int BidId,
    int CriteriaId,
    int EvaluatorId,
    decimal Score,
    string Remarks,
    DateTime EvaluatedAt
);

// Response DTO for finalized tender results
public record TenderResultResponseDto(
    int Id,
    int TenderId,
    int WinningBidId,
    int AwardedByOfficerId,
    DateTime AwardedAt,
    string Remarks
);