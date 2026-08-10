namespace TmsApi.Application.Common.Interfaces;

using TmsApi.Application.Bids.DTOs;
using TmsApi.Domain.Entities;

public interface IBidService
{
    Task<BidResponseDto> SubmitBidAsync(SubmitBidRequestDto dto, CancellationToken cancellationToken = default);
    Task<BidDocumentDto> AddBidDocumentAsync(AddBidDocumentRequestDto dto, CancellationToken cancellationToken = default);
    Task<BidResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<BidResponseDto>> GetBidsByTenderIdAsync(int tenderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BidResponseDto>> GetBidsBySupplierIdAsync(int supplierId, CancellationToken cancellationToken = default);
    Task<bool> UpdateBidStatusAsync(int id, BidStatus status, CancellationToken cancellationToken = default);
}