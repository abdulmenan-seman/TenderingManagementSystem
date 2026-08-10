namespace TmsApi.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Evaluations.DTOs;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

public class BidEvaluationService : IBidEvaluationService
{
    private readonly TmsDbContext _context;

    public BidEvaluationService(TmsDbContext context)
    {
        _context = context;
    }

    public async Task<BidEvaluationResponseDto> SubmitEvaluationAsync(SubmitBidEvaluationRequestDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Check Bid existence
        var bid = await _context.Bids.FirstOrDefaultAsync(b => b.Id == dto.BidId && !b.IsDeleted, cancellationToken);
        if (bid == null)
        {
            throw new KeyNotFoundException($"Bid with ID {dto.BidId} was not found.");
        }

        // 2. Check Criteria existence
        var criteria = await _context.EvaluationCriteria.FirstOrDefaultAsync(c => c.Id == dto.CriteriaId, cancellationToken);
        if (criteria == null)
        {
            throw new KeyNotFoundException($"Evaluation Criteria with ID {dto.CriteriaId} was not found.");
        }

        // 3. Check Evaluator User existence
        var evaluatorExists = await _context.Users.AnyAsync(u => u.Id == dto.EvaluatorId && !u.IsDeleted, cancellationToken);
        if (!evaluatorExists)
        {
            throw new KeyNotFoundException($"Evaluator user with ID {dto.EvaluatorId} was not found.");
        }

        // 4. Score boundary check against criterion MaxScore
        if (dto.Score > criteria.MaxScore)
        {
            throw new InvalidOperationException($"Score ({dto.Score}) cannot exceed maximum score ({criteria.MaxScore}) for this criterion.");
        }

        var evaluation = new BidEvaluation(
            dto.BidId,
            dto.EvaluatorId,
            dto.CriteriaId,
            dto.Score,
            dto.Remarks
        );

        _context.BidEvaluations.Add(evaluation);

        // Update bid status to UnderReview if currently Submitted
        if (bid.Status == BidStatus.Submitted)
        {
            bid.UpdateStatus(BidStatus.UnderReview);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(evaluation);
    }

    public async Task<IEnumerable<BidEvaluationResponseDto>> GetEvaluationsByBidIdAsync(int bidId, CancellationToken cancellationToken = default)
    {
        var evaluations = await _context.BidEvaluations
            .Where(e => e.BidId == bidId)
            .ToListAsync(cancellationToken);

        return evaluations.Select(MapToDto);
    }

    public async Task<TenderResultResponseDto> FinalizeTenderResultAsync(FinalizeTenderResultRequestDto dto, CancellationToken cancellationToken = default)
    {
        var tender = await _context.Tenders.FirstOrDefaultAsync(t => t.Id == dto.TenderId && !t.IsDeleted, cancellationToken);
        if (tender == null)
        {
            throw new KeyNotFoundException($"Tender with ID {dto.TenderId} was not found.");
        }

        var winningBid = await _context.Bids.FirstOrDefaultAsync(b => b.Id == dto.WinningBidId && b.TenderId == dto.TenderId && !b.IsDeleted, cancellationToken);
        if (winningBid == null)
        {
            throw new KeyNotFoundException($"Winning Bid with ID {dto.WinningBidId} does not belong to Tender ID {dto.TenderId}.");
        }

        // Award tender via domain method
        tender.AwardToBid(dto.WinningBidId);

        // Mark winning bid as Awarded, reject remaining bids for this tender
        winningBid.UpdateStatus(BidStatus.Awarded);

        var otherBids = await _context.Bids
            .Where(b => b.TenderId == dto.TenderId && b.Id != dto.WinningBidId && !b.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var otherBid in otherBids)
        {
            otherBid.UpdateStatus(BidStatus.Rejected);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new TenderResultResponseDto(
            tender.Id,
            tender.Id,
            dto.WinningBidId,
            dto.AwardedByOfficerId,
            DateTime.UtcNow,
            dto.Remarks
        );
    }

    private static BidEvaluationResponseDto MapToDto(BidEvaluation evaluation)
    {
        return new BidEvaluationResponseDto(
            evaluation.Id,
            evaluation.BidId,
            evaluation.EvaluationCriteriaId,
            evaluation.EvaluatorId,
            evaluation.AssignedScore,
            evaluation.Comments ?? string.Empty,
            evaluation.EvaluatedAt
        );
    }
}