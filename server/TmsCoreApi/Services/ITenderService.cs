using System.Collections.Generic;
using System.Threading.Tasks;
using TmsCoreApi.Models;

namespace TmsCoreApi.Services;

public interface ITenderService
{
    Task<IEnumerable<Tender>> GetAllAsync();
    Task<Tender?> GetByIdAsync(string id);
    Task<Tender> CreateAsync(Tender tender);
    Task<bool> UpdateStatusAsync(string id, TenderStatus newStatus);
    Task<bool> AssignWinnerAsync(string id, string supplierId);
}