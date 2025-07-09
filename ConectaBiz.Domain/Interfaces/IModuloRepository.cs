using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Interfaces
{
    public interface IModuloRepository
    {
        Task<List<Modulo>> GetAllAsync();
        Task<List<RolPermisoModulo>> GetByRolAsync(int idRol);
    }

}
