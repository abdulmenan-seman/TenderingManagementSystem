namespace TmsApi.Application.Common.Interfaces;

using TmsApi.Application.Tenders.DTOs;
using TmsApi.Domain.Entities;

public interface ITenderService
{
    Task<TenderResponseDto> CreateTenderAsync(CreateTenderRequestDto dto, CancellationToken cancellationToken = default);
    Task<TenderDocumentDto> UploadTenderDocumentAsync(UploadTenderDocumentRequestDto dto, CancellationToken cancellationToken = default);
    Task<TenderResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TenderResponseDto>> GetAllAsync(TenderStatus? status = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<TenderResponseDto>> GetByOfficerIdAsync(int officerId, CancellationToken cancellationToken = default);
    Task<bool> PublishTenderAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> CloseTenderAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AwardTenderAsync(int id, int bidId, CancellationToken cancellationToken = default);
}