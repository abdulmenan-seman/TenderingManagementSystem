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

    public SubmitBidEvaluationCommandHandler(ITmsDbContext context) => _context = context;

    public async Task<Result<int>> Handle(SubmitBidEvaluationCommand request, CancellationToken cancellationToken)
    {
        var dto = request.EvaluationDto;

        var bidExists = await _context.Bids.FindAsync(new object[] { dto.BidId }, cancellationToken);
        if (bidExists is null)
            return Result<int>.Failure($"Bid with ID {dto.BidId} was not found.");

        var criteriaExists = await _context.EvaluationCriteria.FindAsync(new object[] { dto.CriteriaId }, cancellationToken);
        if (criteriaExists is null)
            return Result<int>.Failure($"Evaluation criteria with ID {dto.CriteriaId} was not found.");

        var evaluatorExists = await _context.Users.FindAsync(new object[] { dto.EvaluatorId }, cancellationToken);
        if (evaluatorExists is null)
            return Result<int>.Failure($"Evaluator with ID {dto.EvaluatorId} was not found.");

        try
        {
            var evaluation = new BidEvaluation(
                dto.BidId,
                dto.EvaluatorId,
                dto.CriteriaId,
                dto.Score,
                dto.Remarks
            );

            _context.BidEvaluations.Add(evaluation);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(evaluation.Id);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return Result<int>.Failure(ex.Message);
        }
    }
}