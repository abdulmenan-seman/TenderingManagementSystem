namespace TmsApi.Application.Criteria.Queries.GetCriteriaByTenderId;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;
using TmsApi.Application.Criteria.DTOs;

public record GetCriteriaByIdQuery(int Id) : IRequest<Result<EvaluationCriteriaResponseDto>>;

public class GetCriteriaByIdQueryHandler : IRequestHandler<GetCriteriaByIdQuery, Result<EvaluationCriteriaResponseDto>>
{
    private readonly ITmsDbContext _context;

    public GetCriteriaByIdQueryHandler(ITmsDbContext context) => _context = context;

    public async Task<Result<EvaluationCriteriaResponseDto>> Handle(GetCriteriaByIdQuery request, CancellationToken cancellationToken)
    {
        var criteria = await _context.EvaluationCriteria
            .AsNoTracking()
            .Where(ec => ec.Id == request.Id)
            .Select(ec => new EvaluationCriteriaResponseDto(
                ec.Id,
                ec.TenderId,
                ec.CriteriaName,
                ec.Description,
                ec.WeightPercentage,
                ec.MaxScore
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (criteria is null)
        {
            return Result<EvaluationCriteriaResponseDto>.Failure($"Evaluation criteria with ID {request.Id} was not found.");
        }

        return Result<EvaluationCriteriaResponseDto>.Success(criteria);
    }
}

public record GetCriteriaByTenderIdQuery(int TenderId) : IRequest<Result<List<EvaluationCriteriaResponseDto>>>;

public class GetCriteriaByTenderIdQueryHandler : IRequestHandler<GetCriteriaByTenderIdQuery, Result<List<EvaluationCriteriaResponseDto>>>
{
    private readonly ITmsDbContext _context;

    public GetCriteriaByTenderIdQueryHandler(ITmsDbContext context) => _context = context;

    public async Task<Result<List<EvaluationCriteriaResponseDto>>> Handle(GetCriteriaByTenderIdQuery request, CancellationToken cancellationToken)
    {
        var criteria = await _context.EvaluationCriteria
            .AsNoTracking()
            .Where(ec => ec.TenderId == request.TenderId)
            .Select(ec => new EvaluationCriteriaResponseDto(
                ec.Id,
                ec.TenderId,
                ec.CriteriaName,
                ec.Description,
                ec.WeightPercentage,
                ec.MaxScore
            ))
            .ToListAsync(cancellationToken);

        return Result<List<EvaluationCriteriaResponseDto>>.Success(criteria);
    }
}