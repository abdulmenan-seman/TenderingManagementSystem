namespace TmsApi.Application.Common.Interfaces;

using TmsApi.Domain.Entities;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, IEnumerable<string> roles);
}