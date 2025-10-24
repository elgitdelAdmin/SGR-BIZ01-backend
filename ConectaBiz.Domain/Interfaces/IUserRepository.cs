using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByIdAsync(int id);
        Task<IEnumerable<User>> GetUsersByIdAsync(int[] ids);
        Task<User> GetByIdSocioIdRolIdPersonaAsync(int idsocio, int idrol, int idpersona);
        Task<User> CreateAsync(User user);
        Task UpdateAsync(User user);
        Task<User> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task AddRefreshTokenAsync(RefreshToken refreshToken);
        Task RevokeRefreshTokenAsync(string token);
        Task<IEnumerable<User>> GetAllAsync();
        Task<IEnumerable<User>> GetAllUsuarioByIdSocio(int idSocio);
        Task<IEnumerable<Rol>> GetAllRolAsync();
        Task<Rol> GetRolByIdAsync(int id);
        Task<Rol>GetRolByCodigoAsync(string codigo);
    }
}
