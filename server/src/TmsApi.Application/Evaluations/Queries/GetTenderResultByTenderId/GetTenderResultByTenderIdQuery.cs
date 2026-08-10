namespace TmsApi.Application.Evaluations.Queries.GetTenderResultByTenderId;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;
using TmsApi.Application.Evaluations.DTOs;

public record GetTenderResultByTenderIdQuery(int TenderId) : IRequest<Result<TenderResultResponseDto>>;

public class GetTenderResultByTenderIdQueryHandler : IRequestHandler<GetTenderResultByTenderIdQuery, Result<TenderResultResponseDto>>
{
    private readonly ITmsDbContext _context;

    public GetTenderResultByTenderIdQueryHandler(ITmsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TenderResultResponseDto>> Handle(GetTenderResultByTenderIdQuery request, CancellationToken cancellationToken)
    {
        var resultDto = await _context.TenderResults
            .AsNoTracking()
            .Where(tr => tr.TenderId == request.TenderId)
            .Select(tr => new TenderResultResponseDto(
                tr.Id,
                tr.TenderId,
                tr.WinningBidId,
                tr.ApprovedByOfficerId, // Fixed property name
                tr.AwardedAt,
                tr.SummaryNotes ?? string.Empty
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (resultDto is null)
        {
            return Result<TenderResultResponseDto>.Failure($"No result has been finalized for Tender ID {request.TenderId}.");
        }

        return Result<TenderResultResponseDto>.Success(resultDto);
    }
}