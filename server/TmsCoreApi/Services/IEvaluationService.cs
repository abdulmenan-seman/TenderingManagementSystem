using System.Collections.Generic;
using System.Threading.Tasks;
using TmsCoreApi.Models;

namespace TmsCoreApi.Services;

public interface IEvaluationService
{
    Task<IEnumerable<Evaluation>> GetEvaluationsByBidIdAsync(string bidId);
    Task<Evaluation?> GetByIdAsync(string id);
    Task<Evaluation> SubmitEvaluationAsync(Evaluation evaluation);
}