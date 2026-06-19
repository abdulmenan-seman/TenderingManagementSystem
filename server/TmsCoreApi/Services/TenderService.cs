using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TmsCoreApi.Models;

namespace TmsCoreApi.Services;

public class TenderService(ILogger<TenderService> logger) : ITenderService
{
    private readonly ConcurrentDictionary<string, Tender> _tenders = new();

    public Task<IEnumerable<Tender>> GetAllAsync()
    {
        logger.LogInformation("Fetching total active and draft tender parameters.");
        return Task.FromResult(_tenders.Values.AsEnumerable());
    }

    public Task<Tender?> GetByIdAsync(string id)
    {
        var standardizedId = id.ToUpperInvariant();
        if (!_tenders.TryGetValue(standardizedId, out var tender))
        {
            logger.LogWarning("Tender document query failed for ID {TenderId}.", standardizedId);
            return Task.FromResult<Tender?>(null);
        }
        return Task.FromResult<Tender?>(tender);
    }

    public Task<Tender> CreateAsync(Tender tender)
    {
        var standardizedId = tender.Id.ToUpperInvariant();
        if (_tenders.ContainsKey(standardizedId))
        {
            logger.LogWarning("Tender validation failed. A tender record with ID {TenderId} already exists.", standardizedId);
            throw new ArgumentException($"Tender index path {standardizedId} is occupied.");
        }

        var savedTender = new Tender
        {
            Id = standardizedId,
            Title = tender.Title,
            Description = tender.Description,
            BudgetLimit = tender.BudgetLimit,
            SubmissionDeadline = tender.SubmissionDeadline,
            Status = TenderStatus.Draft,
            CreatedByOfficerId = tender.CreatedByOfficerId
        };

        _tenders[standardizedId] = savedTender;
        logger.LogInformation("New tender '{TenderTitle}' successfully logged under ID {TenderId}.", savedTender.Title, standardizedId);
        return Task.FromResult(savedTender);
    }

    public Task<bool> UpdateStatusAsync(string id, TenderStatus newStatus)
    {
        var standardizedId = id.ToUpperInvariant();
        if (!_tenders.TryGetValue(standardizedId, out var tender))
        {
            logger.LogWarning("Status conversion stopped. Tender ID {TenderId} missing.", standardizedId);
            return Task.FromResult(false);
        }

        tender.Status = newStatus;
        logger.LogInformation("Tender state flag matching ID {TenderId} updated safely to '{TenderStatus}'.", standardizedId, newStatus);
        return Task.FromResult(true);
    }

    public Task<bool> AssignWinnerAsync(string id, string supplierId)
    {
        var standardizedId = id.ToUpperInvariant();
        if (!_tenders.TryGetValue(standardizedId, out var tender))
        {
            logger.LogWarning("Award sequence broken. Tender profile ID {TenderId} could not be resolved.", standardizedId);
            return Task.FromResult(false);
        }

        tender.WinningBidderId = supplierId;
        tender.Status = TenderStatus.Awarded;
        logger.LogInformation("Tender {TenderId} officially awarded successfully to Supplier {SupplierId}.", standardizedId, supplierId);
        return Task.FromResult(true);
    }
}