namespace TmsApi.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(int userId, string email, string fullName, IList<string> roles);
}