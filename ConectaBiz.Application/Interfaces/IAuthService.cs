using ConectaBiz.Application.DTOs;
using ConectaBiz.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequestDto loginRequest);
        Task<AuthResponseDto> RegisterAsync(RegisterUserDto registerRequest);
        Task<UserDto> UpdateUserAsync(UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int id);
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<IEnumerable<UserDto>> GetAllUsuarioByIdSocio(int idSocio);
        Task<UserDto> GetByIdAsync(int id);
        Task<UserDto> GetByIdSocioIdRolIdAsync(int idsocio, int idrol, int idpersona);
        Task<IEnumerable<RolDto>> GetAllRolAsync();
        Task<RolDto> GetRolByCodigoAsync(string codigo);
    }
}
