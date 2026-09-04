namespace TmsApi.Application.Tenders.Handlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;
using TmsApi.Application.Tenders.DTOs;
using TmsApi.Application.Tenders.QueriesAndCommands;

public class GetTendersQueryHandler : IRequestHandler<GetTendersQuery, Result<PaginatedTendersDto>>
{
    private readonly ITmsDbContext _context;

    public GetTendersQueryHandler(ITmsDbContext context) => _context = context;

    public async Task<Result<PaginatedTendersDto>> Handle(GetTendersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Tenders
            .Include(t => t.Documents)
            .Where(t => !t.IsDeleted)
            .AsQueryable();

        // Filter by status string if provided
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<Domain.Entities.TenderStatus>(request.Status, true, out var parsedStatus))
        {
            query = query.Where(t => t.Status == parsedStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(t => new TenderSummaryDto(
            t.Id,
            t.ReferenceNumber,
            t.Title,
            t.EstimatedBudget,
            t.SubmissionDeadline,
            t.Status.ToString(),
            t.CreatedByOfficerId,
            t.Documents.Select(d => new TenderDocumentDto(d.Id, d.FileName, d.FilePath, d.UploadedAt)).ToList()
        )).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return Result<PaginatedTendersDto>.Success(new PaginatedTendersDto(
            dtos, request.PageNumber, request.PageSize, totalCount, totalPages));
    }
}
