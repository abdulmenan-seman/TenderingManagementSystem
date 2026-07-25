namespace TmsApi.Application.Criteria.DTOs;

// Incoming request payload from frontend/client
public record AddEvaluationCriteriaRequestDto(
    int TenderId,
    string CriteriaName,
    string Description,
    decimal WeightPercentage,
    decimal MaxScore
);

// Outgoing response DTO for API clients
public record EvaluationCriteriaResponseDto(
    int Id,
    int TenderId,
    string CriteriaName,
    string Description,
    decimal WeightPercentage,
    decimal MaxScore
);