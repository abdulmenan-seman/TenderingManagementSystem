namespace TmsApi.Application.Common.Interfaces;

using TmsApi.Application.Criteria.DTOs;

public interface IEvaluationCriteriaService
{
    Task<EvaluationCriteriaResponseDto> AddCriteriaAsync(AddEvaluationCriteriaRequestDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<EvaluationCriteriaResponseDto>> GetByTenderIdAsync(int tenderId, CancellationToken cancellationToken = default);
    Task<EvaluationCriteriaResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> DeleteCriteriaAsync(int id, CancellationToken cancellationToken = default);
}