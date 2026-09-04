namespace TmsApi.Application.Bids.Queries.GetBidsByTenderId;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Bids.DTOs;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;

public record GetBidsByTenderIdQuery(int TenderId) : IRequest<Result<List<BidResponseDto>>>;

public class GetBidsByTenderIdQueryHandler : IRequestHandler<GetBidsByTenderIdQuery, Result<List<BidResponseDto>>>
{
    private readonly ITmsDbContext _context;

    public GetBidsByTenderIdQueryHandler(ITmsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<BidResponseDto>>> Handle(GetBidsByTenderIdQuery request, CancellationToken cancellationToken)
    {
        var bids = await _context.Bids
            .AsNoTracking()
            .Where(b => b.TenderId == request.TenderId)
            .Select(b => new BidResponseDto(
                b.Id,
                b.TenderId,
                b.SupplierId,
                b.FinancialProposalAmount,
                b.SubmissionDate,
                b.Status.ToString(),
                _context.BidDocuments
                    .Where(bd => bd.BidId == b.Id)
                    .Select(bd => new BidDocumentDto(
                        bd.Id,
                        bd.DocumentType,
                        bd.FileName,
                        $"/api/v1/bids/documents/{bd.Id}/download",
                        bd.UploadedAt
                    ))
                    .ToList()
            ))
            .ToListAsync(cancellationToken);

        return Result<List<BidResponseDto>>.Success(bids);
    }
}