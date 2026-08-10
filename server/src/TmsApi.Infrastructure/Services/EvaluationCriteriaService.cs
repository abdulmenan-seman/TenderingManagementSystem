namespace TmsApi.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Criteria.DTOs;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

public class EvaluationCriteriaService : IEvaluationCriteriaService
{
    private readonly TmsDbContext _context;

    public EvaluationCriteriaService(TmsDbContext context)
    {
        _context = context;
    }

    public async Task<EvaluationCriteriaResponseDto> AddCriteriaAsync(AddEvaluationCriteriaRequestDto dto, CancellationToken cancellationToken = default)
    {
        var tenderExists = await _context.Tenders
            .AnyAsync(t => t.Id == dto.TenderId && !t.IsDeleted, cancellationToken);
        if (!tenderExists)
        {
            throw new KeyNotFoundException($"Tender with ID {dto.TenderId} was not found.");
        }

        // Ensure total weight percentage for this tender does not exceed 100%
        var currentTotalWeight = await _context.EvaluationCriteria
            .Where(c => c.TenderId == dto.TenderId)
            .SumAsync(c => c.WeightPercentage, cancellationToken);

        if (currentTotalWeight + dto.WeightPercentage > 100)
        {
            throw new InvalidOperationException($"Adding this criteria exceeds total weight of 100%. Current total: {currentTotalWeight}%.");
        }

        var criteria = new EvaluationCriteria(
            dto.TenderId,
            dto.CriteriaName,
            dto.Description,
            dto.WeightPercentage,
            dto.MaxScore
        );

        _context.EvaluationCriteria.Add(criteria);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(criteria);
    }

    public async Task<IEnumerable<EvaluationCriteriaResponseDto>> GetByTenderIdAsync(int tenderId, CancellationToken cancellationToken = default)
    {
        var criteriaList = await _context.EvaluationCriteria
            .Where(c => c.TenderId == tenderId)
            .ToListAsync(cancellationToken);

        return criteriaList.Select(MapToDto);
    }

    public async Task<EvaluationCriteriaResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var criteria = await _context.EvaluationCriteria
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        return criteria == null ? null : MapToDto(criteria);
    }

    public async Task<bool> DeleteCriteriaAsync(int id, CancellationToken cancellationToken = default)
    {
        var criteria = await _context.EvaluationCriteria.FindAsync(new object[] { id }, cancellationToken);
        if (criteria == null) return false;

        _context.EvaluationCriteria.Remove(criteria);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static EvaluationCriteriaResponseDto MapToDto(EvaluationCriteria criteria)
    {
        return new EvaluationCriteriaResponseDto(
            criteria.Id,
            criteria.TenderId,
            criteria.CriteriaName,
            criteria.Description,
            criteria.WeightPercentage,
            criteria.MaxScore
        );
    }
}