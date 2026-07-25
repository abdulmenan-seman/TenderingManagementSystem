namespace TmsApi.Application.Evaluations.Commands.SubmitBidEvaluation;

using MediatR;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;
using TmsApi.Application.Evaluations.DTOs;
using TmsApi.Domain.Entities;

public record SubmitBidEvaluationCommand(SubmitBidEvaluationRequestDto EvaluationDto) : IRequest<Result<int>>;

public class SubmitBidEvaluationCommandHandler : IRequestHandler<SubmitBidEvaluationCommand, Result<int>>
{
    private readonly ITmsDbContext _context;

    public SubmitBidEvaluationCommandHandler(ITmsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(SubmitBidEvaluationCommand request, CancellationToken cancellationToken)
    {
        var dto = request.EvaluationDto;

        // Verify Bid exists
        var bid = await _context.Bids.FindAsync(new object[] { dto.BidId }, cancellationToken);
        if (bid is null)
        {
            return Result<int>.Failure($"Bid with ID {dto.BidId} was not found.");
        }

        // Verify Evaluation Criteria exists
        var criteria = await _context.EvaluationCriteria.FindAsync(new object[] { dto.CriteriaId }, cancellationToken);
        if (criteria is null)
        {
            return Result<int>.Failure($"Evaluation Criteria with ID {dto.CriteriaId} was not found.");
        }

        // Validate max score rule
        if (dto.Score > criteria.MaxScore)
        {
            return Result<int>.Failure($"Score ({dto.Score}) cannot exceed the maximum allowed score ({criteria.MaxScore}) for this criteria.");
        }

        try
        {
            var evaluation = new BidEvaluation(
                dto.BidId,
                dto.CriteriaId,
                dto.EvaluatorId,
                dto.Score,
                dto.Remarks
            );

            _context.BidEvaluations.Add(evaluation);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(evaluation.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(ex.Message);
        }
    }
}