namespace TmsApi.Application.Tenders.Handlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;
using TmsApi.Application.Tenders.DTOs;
using TmsApi.Application.Tenders.QueriesAndCommands;

public class GetTenderByIdQueryHandler : IRequestHandler<GetTenderByIdQuery, Result<TenderResponseDto>>
{
    private readonly ITmsDbContext _context;

    public GetTenderByIdQueryHandler(ITmsDbContext context) => _context = context;

    public async Task<Result<TenderResponseDto>> Handle(GetTenderByIdQuery request, CancellationToken cancellationToken)
    {
        // Query Database with projection for optimal query execution
        var tender = await _context.Tenders
            .AsNoTracking()
            .Include(t => t.Documents)
            .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken);

        if (tender is null)
            return Result<TenderResponseDto>.Failure($"Tender with ID {request.Id} was not found.");

        var documentDtos = tender.Documents
            .Select(d => new TenderDocumentDto(d.Id, d.FileName, d.FilePath, d.UploadedAt))
            .ToList();

        var response = new TenderResponseDto(
            tender.Id,
            tender.ReferenceNumber,
            tender.Title,
            tender.Description,
            tender.EstimatedBudget,
            tender.PublicationDate,
            tender.SubmissionDeadline,
            tender.Status.ToString(),
            tender.CreatedByOfficerId,
            documentDtos
        );

        return Result<TenderResponseDto>.Success(response);
    }
}