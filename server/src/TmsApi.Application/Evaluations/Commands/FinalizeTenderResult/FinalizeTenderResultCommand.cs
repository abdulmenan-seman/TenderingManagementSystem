namespace TmsApi.Application.Evaluations.Commands.FinalizeTenderResult;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;
using TmsApi.Application.Evaluations.DTOs;
using TmsApi.Domain.Entities;

public record FinalizeTenderResultCommand(FinalizeTenderResultRequestDto ResultDto) : IRequest<Result<int>>;

public class FinalizeTenderResultCommandHandler : IRequestHandler<FinalizeTenderResultCommand, Result<int>>
{
    private readonly ITmsDbContext _context;

    public FinalizeTenderResultCommandHandler(ITmsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(FinalizeTenderResultCommand request, CancellationToken cancellationToken)
    {
        var dto = request.ResultDto;

        // 1. Verify Tender
        var tender = await _context.Tenders.FindAsync(new object[] { dto.TenderId }, cancellationToken);
        if (tender is null)
        {
            return Result<int>.Failure($"Tender with ID {dto.TenderId} was not found.");
        }

        // 2. Verify Winning Bid
        var winningBid = await _context.Bids.FindAsync(new object[] { dto.WinningBidId }, cancellationToken);
        if (winningBid is null || winningBid.TenderId != dto.TenderId)
        {
            return Result<int>.Failure($"Bid with ID {dto.WinningBidId} is invalid for Tender {dto.TenderId}.");
        }

        try
        {
            // 1. Create Tender Result record
            var tenderResult = new TenderResult(
                dto.TenderId,
                dto.WinningBidId,
                dto.AwardedByOfficerId,
                winningBid.FinancialProposalAmount,
                dto.Remarks
            );
            _context.TenderResults.Add(tenderResult);

            // 2. Notify Winning Supplier
            var winnerNotification = new Notification(
                winningBid.SupplierId,
                $"Congratulations! Your bid for Tender '{tender.Title}' has been awarded.",
                "TenderAward"
            );
            _context.Notifications.Add(winnerNotification);

            // 3. Notify Other Bidders (Non-winning)
            var otherBids = await _context.Bids
                .Where(b => b.TenderId == dto.TenderId && b.Id != dto.WinningBidId)
                .ToListAsync(cancellationToken);

            foreach (var bid in otherBids)
            {
                var rejectionNotification = new Notification(
                    bid.SupplierId,
                    $"The evaluation for Tender '{tender.Title}' has been finalized. Thank you for your submission.",
                    "TenderResult"
                );
                _context.Notifications.Add(rejectionNotification);
            }

            // 4. Create Audit Log (Matching domain types: string userId, string action, int? entityId, string entityName, string details)
            var auditLog = new AuditLog(
                dto.AwardedByOfficerId.ToString(), // Converted int to string
                "AwardTender",
                dto.TenderId,                      // Passed int? for EntityId
                "Tender",                          // Passed string for EntityName
                $"Tender #{dto.TenderId} was awarded to Bid #{dto.WinningBidId}."
            );
            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(tenderResult.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(ex.Message);
        }
    }
}