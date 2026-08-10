namespace TmsApi.Application.Tenders.QueriesAndCommands;

using MediatR;
using TmsApi.Application.Common.Models;
using TmsApi.Application.Tenders.DTOs;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Domain.Entities;

public record CreateTenderCommand(CreateTenderRequestDto Dto) : IRequest<TenderResponseDto>;
public record UploadTenderDocumentCommand(UploadTenderDocumentRequestDto Dto) : IRequest<TenderDocumentDto>;
public record GetTenderByIdQuery(int Id) : IRequest<TenderResponseDto?>;
public record GetPagedTendersQuery(TenderStatus? Status, PaginationParams Pagination) : IRequest<PagedResponse<TenderResponseDto>>;
public record PublishTenderCommand(int Id) : IRequest<bool>;
public record CloseTenderCommand(int Id) : IRequest<bool>;
public record AwardTenderCommand(int Id, int BidId) : IRequest<bool>;

public class TenderHandler :
    IRequestHandler<CreateTenderCommand, TenderResponseDto>,
    IRequestHandler<UploadTenderDocumentCommand, TenderDocumentDto>,
    IRequestHandler<GetTenderByIdQuery, TenderResponseDto?>,
    IRequestHandler<GetPagedTendersQuery, PagedResponse<TenderResponseDto>>,
    IRequestHandler<PublishTenderCommand, bool>,
    IRequestHandler<CloseTenderCommand, bool>,
    IRequestHandler<AwardTenderCommand, bool>
{
    private readonly ITenderService _tenderService;

    public TenderHandler(ITenderService tenderService) => _tenderService = tenderService;

    public Task<TenderResponseDto> Handle(CreateTenderCommand request, CancellationToken cancellationToken)
        => _tenderService.CreateTenderAsync(request.Dto, cancellationToken);

    public Task<TenderDocumentDto> Handle(UploadTenderDocumentCommand request, CancellationToken cancellationToken)
        => _tenderService.UploadTenderDocumentAsync(request.Dto, cancellationToken);

    public Task<TenderResponseDto?> Handle(GetTenderByIdQuery request, CancellationToken cancellationToken)
        => _tenderService.GetByIdAsync(request.Id, cancellationToken);

    public async Task<PagedResponse<TenderResponseDto>> Handle(GetPagedTendersQuery request, CancellationToken cancellationToken)
    {
        var allTenders = (await _tenderService.GetAllAsync(request.Status, cancellationToken)).ToList();
        var totalCount = allTenders.Count;

        var items = allTenders
            .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize)
            .ToList();

        return new PagedResponse<TenderResponseDto>(items, request.Pagination.PageNumber, request.Pagination.PageSize, totalCount);
    }

    public Task<bool> Handle(PublishTenderCommand request, CancellationToken cancellationToken)
        => _tenderService.PublishTenderAsync(request.Id, cancellationToken);

    public Task<bool> Handle(CloseTenderCommand request, CancellationToken cancellationToken)
        => _tenderService.CloseTenderAsync(request.Id, cancellationToken);

    public Task<bool> Handle(AwardTenderCommand request, CancellationToken cancellationToken)
        => _tenderService.AwardTenderAsync(request.Id, request.BidId, cancellationToken);
}