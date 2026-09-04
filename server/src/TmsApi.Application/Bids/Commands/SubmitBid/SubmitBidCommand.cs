namespace TmsApi.Application.Bids.Commands.SubmitBid;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Bids.DTOs;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;
using TmsApi.Domain.Entities;

public record SubmitBidCommand(int SupplierId, SubmitBidRequestDto BidDto) : IRequest<Result<int>>;

public class SubmitBidCommandHandler : IRequestHandler<SubmitBidCommand, Result<int>>
{
    private readonly ITmsDbContext _context;

    public SubmitBidCommandHandler(ITmsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(SubmitBidCommand request, CancellationToken cancellationToken)
    {
        var dto = request.BidDto;

        // Verify Tender exists
        var tender = await _context.Tenders.FirstOrDefaultAsync(t => t.Id == dto.TenderId && !t.IsDeleted, cancellationToken);
        if (tender is null)
        {
            return Result<int>.Failure($"Tender with ID {dto.TenderId} was not found.");
        }

        if (tender.Status != TenderStatus.Published)
            return Result<int>.Failure("Bids can only be submitted for published tenders.");

        if (DateTime.UtcNow > tender.SubmissionDeadline)
        {
            return Result<int>.Failure("The submission deadline for this tender has passed.");
        }



        try
        {
            var bid = new Bid(
                dto.TenderId,
                request.SupplierId,
                dto.FinancialProposalAmount
            );

            _context.Bids.Add(bid);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(bid.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(ex.Message);
        }
    }
}