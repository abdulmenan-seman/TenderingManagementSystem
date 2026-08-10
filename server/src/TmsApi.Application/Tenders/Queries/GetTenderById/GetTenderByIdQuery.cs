namespace TmsApi.Application.Tenders.Queries.GetTenderById;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;
using TmsApi.Application.Tenders.DTOs;

public record GetTenderByIdQuery(int Id) : IRequest<Result<TenderResponseDto>>;

public class GetTenderByIdQueryHandler : IRequestHandler<GetTenderByIdQuery, Result<TenderResponseDto>>
{
    private readonly ITmsDbContext _context;

    public GetTenderByIdQueryHandler(ITmsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TenderResponseDto>> Handle(GetTenderByIdQuery request, CancellationToken cancellationToken)
    {
        var tenderDto = await _context.Tenders
            .AsNoTracking()
            .Where(t => t.Id == request.Id)
            .Select(t => new TenderResponseDto(
                t.Id,
                t.ReferenceNumber,
                t.Title,
                t.Description,
                t.EstimatedBudget,
                t.PublicationDate,
                t.SubmissionDeadline,
                t.Status.ToString(),
                t.CreatedByOfficerId,
                _context.TenderDocuments
                    .Where(d => d.TenderId == t.Id)
                    .Select(d => new TenderDocumentDto(d.Id, d.FileName, d.FilePath, d.UploadedAt))
                    .ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (tenderDto is null)
        {
            return Result<TenderResponseDto>.Failure($"Tender with ID {request.Id} was not found.");
        }

        return Result<TenderResponseDto>.Success(tenderDto);
    }
}