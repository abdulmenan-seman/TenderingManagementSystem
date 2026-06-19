using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TmsCoreApi.Models;

namespace TmsCoreApi.Services;

public class EvaluationService(ILogger<EvaluationService> logger) : IEvaluationService
{
    private readonly ConcurrentDictionary<string, Evaluation> _evaluations = new();

    public Task<IEnumerable<Evaluation>> GetEvaluationsByBidIdAsync(string bidId)
    {
        var targetBidId = bidId.ToUpperInvariant();
        var dataCollection = _evaluations.Values.Where(e => e.BidId.Equals(targetBidId, StringComparison.OrdinalIgnoreCase));
        logger.LogInformation("Committee record review requested for target Bid identity {BidId}.", targetBidId);
        return Task.FromResult(dataCollection);
    }

    public Task<Evaluation?> GetByIdAsync(string id)
    {
        var standardizedId = id.ToUpperInvariant();
        if (!_evaluations.TryGetValue(standardizedId, out var evaluation))
        {
            logger.LogWarning("Evaluation form report index query failed for ID {EvaluationId}.", standardizedId);
            return Task.FromResult<Evaluation?>(null);
        }
        return Task.FromResult<Evaluation?>(evaluation);
    }

    public Task<Evaluation> SubmitEvaluationAsync(Evaluation evaluation)
    {
        var standardizedId = evaluation.Id.ToUpperInvariant();
        if (_evaluations.ContainsKey(standardizedId))
        {
            logger.LogWarning("Committee submission rejected. Evaluation tracking index {EvaluationId} is occupied.", standardizedId);
            throw new ArgumentException($"Evaluation sheet index {standardizedId} already exists.");
        }

        var savedSheet = new Evaluation
        {
            Id = standardizedId,
            BidId = evaluation.BidId.ToUpperInvariant(),
            EvaluatorId = evaluation.EvaluatorId.ToUpperInvariant(),
            TechnicalScore = evaluation.TechnicalScore,
            Remarks = evaluation.Remarks
        };

        _evaluations[standardizedId] = savedSheet;
        logger.LogInformation("Committee score assessment card {EvaluationId} successfully compiled against Bid ID {BidId}.", standardizedId, savedSheet.BidId);
        return Task.FromResult(savedSheet);
    }
}