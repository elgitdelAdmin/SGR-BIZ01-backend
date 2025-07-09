using ConectaBiz.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Interfaces
{
    public interface IModuloService
    {
        Task<List<ModuloPermisoDto>> GetAllModulosAsync();
        Task<List<ModuloPermisoDto>> GetModulosByRolAsync(int idRol);
    }

}
