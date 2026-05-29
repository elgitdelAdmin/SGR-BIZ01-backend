using ConectaBiz.Domain.Entities;

namespace ConectaBiz.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateAccessToken(User user, int idRol, int idSocio, string rolCodigo);
        string GenerateRefreshToken();
        int? ValidateToken(string token);
    }
}