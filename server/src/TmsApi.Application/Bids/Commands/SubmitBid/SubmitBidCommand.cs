namespace TmsApi.Application.Bids.Commands.SubmitBid;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Bids.DTOs;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;
using TmsApi.Domain.Entities;

public record SubmitBidCommand(SubmitBidRequestDto BidDto) : IRequest<Result<int>>;

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
        var tender = await _context.Tenders.FindAsync(new object[] { dto.TenderId }, cancellationToken);
        if (tender is null)
        {
            return Result<int>.Failure($"Tender with ID {dto.TenderId} was not found.");
        }

        // Business Rule: Cannot submit bids after submission deadline
        if (DateTime.UtcNow > tender.SubmissionDeadline)
        {
            return Result<int>.Failure("The submission deadline for this tender has passed.");
        }



        try
        {
            var bid = new Bid(
                dto.TenderId,
                dto.SupplierId,
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