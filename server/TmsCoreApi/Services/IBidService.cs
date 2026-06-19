using System.Collections.Generic;
using System.Threading.Tasks;
using TmsCoreApi.Models;

namespace TmsCoreApi.Services;

public interface IBidService
{
    Task<IEnumerable<Bid>> GetBidsByTenderIdAsync(string tenderId);
    Task<Bid?> GetByIdAsync(string id);
    Task<Bid> SubmitBidAsync(Bid bid);
    Task<bool> UpdateBidStatusAsync(string id, BidStatus status);
}