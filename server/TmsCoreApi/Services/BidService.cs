using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TmsCoreApi.Models;

namespace TmsCoreApi.Services;

public class BidService(ILogger<BidService> logger) : IBidService
{
    private readonly ConcurrentDictionary<string, Bid> _bids = new();

    public Task<IEnumerable<Bid>> GetBidsByTenderIdAsync(string tenderId)
    {
        var targetId = tenderId.ToUpperInvariant();
        var matchingBids = _bids.Values.Where(b => b.TenderId.Equals(targetId, StringComparison.OrdinalIgnoreCase));
        logger.LogInformation("Querying bids collection for Tender ID {TenderId}.", targetId);
        return Task.FromResult(matchingBids);
    }

    public Task<Bid?> GetByIdAsync(string id)
    {
        var standardizedId = id.ToUpperInvariant();
        if (!_bids.TryGetValue(standardizedId, out var bid))
        {
            logger.LogWarning("Inbound bid verification tracker reported ID {BidId} missing.", standardizedId);
            return Task.FromResult<Bid?>(null);
        }
        return Task.FromResult<Bid?>(bid);
    }

    public Task<Bid> SubmitBidAsync(Bid bid)
    {
        var standardizedId = bid.Id.ToUpperInvariant();
        if (_bids.ContainsKey(standardizedId))
        {
            logger.LogWarning("Electronic bid submission blocked. ID {BidId} is duplicate.", standardizedId);
            throw new ArgumentException($"Bid indexing key {standardizedId} already exists.");
        }

        var recordedBid = new Bid
        {
            Id = standardizedId,
            TenderId = bid.TenderId.ToUpperInvariant(),
            SupplierId = bid.SupplierId.ToUpperInvariant(),
            TechnicalProposalUrl = bid.TechnicalProposalUrl,
            FinancialOffer = bid.FinancialOffer,
            Status = BidStatus.Submitted
        };

        _bids[standardizedId] = recordedBid;
        logger.LogInformation("Bid proposal ID {BidId} successfully registered under Tender Reference {TenderId}.", standardizedId, recordedBid.TenderId);
        return Task.FromResult(recordedBid);
    }

    public Task<bool> UpdateBidStatusAsync(string id, BidStatus status)
    {
        var standardizedId = id.ToUpperInvariant();
        if (!_bids.TryGetValue(standardizedId, out var bid))
        {
            logger.LogWarning("Bid target state manipulation dropped. ID {BidId} invalid.", standardizedId);
            return Task.FromResult(false);
        }

        bid.Status = status;
        logger.LogInformation("Bid identity reference {BidId} shifted cleanly to status context '{BidStatus}'.", standardizedId, status);
        return Task.FromResult(true);
    }
}