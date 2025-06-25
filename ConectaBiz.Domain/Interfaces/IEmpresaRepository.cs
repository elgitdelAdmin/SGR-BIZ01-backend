using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Interfaces
{
    public interface IEmpresaRepository
    {
        Task<IEnumerable<Empresa>> GetAllAsync();
        Task<IEnumerable<Empresa>> GetAllActiveAsync();
        Task<Empresa?> GetByIdAsync(int id);
        Task<Empresa> GetByIdUserAsync(int iduser);
        Task<Empresa?> GetByCodigoAsync(string codigo);
        Task<IEnumerable<Empresa>> GetBySocioAsync(int idSocio);
        Task<IEnumerable<Empresa>> GetByGestorAsync(int idGestor);
        Task<Empresa> CreateAsync(Empresa empresa);
        Task<Empresa> UpdateAsync(Empresa empresa);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByNumDocYPaisAsync(string numDocContribuyente, int? idPais);
        Task<bool> ExistsAsync(int id);
    }

}
