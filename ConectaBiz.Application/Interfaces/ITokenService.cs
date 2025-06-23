using ConectaBiz.Domain.Entities;

namespace ConectaBiz.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        int? ValidateToken(string token);
    }
}