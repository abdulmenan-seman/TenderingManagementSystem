namespace TmsApi.Application.Evaluations.Commands.FinalizeTenderResult;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;
using TmsApi.Application.Evaluations.DTOs;
using TmsApi.Domain.Entities;

public record FinalizeTenderResultCommand(FinalizeTenderResultRequestDto FinalizeDto) : IRequest<Result<TenderResultResponseDto>>;

public class FinalizeTenderResultCommandHandler : IRequestHandler<FinalizeTenderResultCommand, Result<TenderResultResponseDto>>
{
    private readonly ITmsDbContext _context;

    public FinalizeTenderResultCommandHandler(ITmsDbContext context) => _context = context;

    public async Task<Result<TenderResultResponseDto>> Handle(FinalizeTenderResultCommand request, CancellationToken cancellationToken)
    {
        var dto = request.FinalizeDto;

        var tender = await _context.Tenders.FindAsync(new object[] { dto.TenderId }, cancellationToken);
        if (tender is null)
            return Result<TenderResultResponseDto>.Failure($"Tender with ID {dto.TenderId} was not found.");

        var winningBid = await _context.Bids.FindAsync(new object[] { dto.WinningBidId }, cancellationToken);
        if (winningBid is null || winningBid.TenderId != dto.TenderId)
            return Result<TenderResultResponseDto>.Failure($"Winning bid with ID {dto.WinningBidId} is invalid for this tender.");

        // Mark winning bid as Awarded and reject other submitted bids
        var allBids = await _context.Bids.Where(b => b.TenderId == dto.TenderId).ToListAsync(cancellationToken);
        foreach (var bid in allBids)
        {
            if (bid.Id == dto.WinningBidId)
                bid.UpdateStatus(BidStatus.Awarded);
            else if (bid.Status != BidStatus.Disqualified)
                bid.UpdateStatus(BidStatus.Rejected);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var response = new TenderResultResponseDto(
            1, // Result entry record ID
            dto.TenderId,
            dto.WinningBidId,
            dto.AwardedByOfficerId,
            DateTime.UtcNow,
            dto.Remarks
        );

        return Result<TenderResultResponseDto>.Success(response);
    }
}