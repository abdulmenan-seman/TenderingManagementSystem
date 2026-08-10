namespace TmsApi.Application.Common.Interfaces;

using TmsApi.Application.Evaluations.DTOs;

public interface IBidEvaluationService
{
    Task<BidEvaluationResponseDto> SubmitEvaluationAsync(SubmitBidEvaluationRequestDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<BidEvaluationResponseDto>> GetEvaluationsByBidIdAsync(int bidId, CancellationToken cancellationToken = default);
    Task<TenderResultResponseDto> FinalizeTenderResultAsync(FinalizeTenderResultRequestDto dto, CancellationToken cancellationToken = default);
}