using System.Collections.Generic;
using System.Threading.Tasks;
using TmsCoreApi.Models;

namespace TmsCoreApi.Services;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(string id);
    Task<User> RegisterAsync(User user);
    Task<bool> DeactivateAsync(string id);
}