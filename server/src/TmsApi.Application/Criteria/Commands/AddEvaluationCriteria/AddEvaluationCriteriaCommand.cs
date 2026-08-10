namespace TmsApi.Application.Criteria.Commands.AddEvaluationCriteria;

using MediatR;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;
using TmsApi.Application.Criteria.DTOs;
using TmsApi.Domain.Entities;

public record AddEvaluationCriteriaCommand(AddEvaluationCriteriaRequestDto CriteriaDto) : IRequest<Result<int>>;

public class AddEvaluationCriteriaCommandHandler : IRequestHandler<AddEvaluationCriteriaCommand, Result<int>>
{
    private readonly ITmsDbContext _context;

    public AddEvaluationCriteriaCommandHandler(ITmsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(AddEvaluationCriteriaCommand request, CancellationToken cancellationToken)
    {
        var dto = request.CriteriaDto;

        // Verify Tender exists
        var tenderExists = await _context.Tenders.FindAsync(new object[] { dto.TenderId }, cancellationToken);
        if (tenderExists is null)
        {
            return Result<int>.Failure($"Tender with ID {dto.TenderId} does not exist.");
        }

        try
        {
            // Instantiate EvaluationCriteria (executes domain validation rules)
            var criteria = new EvaluationCriteria(
                dto.TenderId,
                dto.CriteriaName,
                dto.Description,
                dto.WeightPercentage,
                dto.MaxScore
            );

            _context.EvaluationCriteria.Add(criteria);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(criteria.Id);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return Result<int>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(ex.Message);
        }
    }
}