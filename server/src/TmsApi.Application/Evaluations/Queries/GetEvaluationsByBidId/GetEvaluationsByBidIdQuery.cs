namespace TmsApi.Application.Evaluations.Queries.GetEvaluationsByBidId;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;
using TmsApi.Application.Evaluations.DTOs;

public record GetEvaluationsByBidIdQuery(int BidId) : IRequest<Result<List<BidEvaluationResponseDto>>>;

public class GetEvaluationsByBidIdQueryHandler : IRequestHandler<GetEvaluationsByBidIdQuery, Result<List<BidEvaluationResponseDto>>>
{
    private readonly ITmsDbContext _context;

    public GetEvaluationsByBidIdQueryHandler(ITmsDbContext context) => _context = context;

    public async Task<Result<List<BidEvaluationResponseDto>>> Handle(GetEvaluationsByBidIdQuery request, CancellationToken cancellationToken)
    {
        var evaluations = await _context.BidEvaluations
            .AsNoTracking()
            .Where(e => e.BidId == request.BidId)
            .Select(e => new BidEvaluationResponseDto(
                e.Id,
                e.BidId,
                e.EvaluationCriteriaId,
                e.EvaluatorId,
                e.AssignedScore,
                e.Comments ?? string.Empty,
                e.EvaluatedAt
            ))
            .ToListAsync(cancellationToken);

        return Result<List<BidEvaluationResponseDto>>.Success(evaluations);
    }
}