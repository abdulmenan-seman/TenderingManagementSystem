namespace TmsApi.Application.Criteria.Commands.DeleteEvaluationCriteria;

using MediatR;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;

public record DeleteEvaluationCriteriaCommand(int Id) : IRequest<Result<bool>>;

public class DeleteEvaluationCriteriaCommandHandler : IRequestHandler<DeleteEvaluationCriteriaCommand, Result<bool>>
{
    private readonly ITmsDbContext _context;

    public DeleteEvaluationCriteriaCommandHandler(ITmsDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(DeleteEvaluationCriteriaCommand request, CancellationToken cancellationToken)
    {
        var criteria = await _context.EvaluationCriteria.FindAsync(new object[] { request.Id }, cancellationToken);
        if (criteria is null)
        {
            return Result<bool>.Failure($"Criteria with ID {request.Id} was not found.");
        }

        _context.EvaluationCriteria.Remove(criteria);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}