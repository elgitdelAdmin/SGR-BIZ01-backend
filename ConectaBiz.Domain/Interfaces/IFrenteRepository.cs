using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Interfaces
{
    public interface IFrenteRepository
    {
        Task<IEnumerable<Frente>> GetAllAsync();
        Task<IEnumerable<Frente>> GetActiveAsync();
        Task<Frente?> GetByIdAsync(int id);
        Task<Frente?> GetByIdWithSubFrentesAsync(int id);
        Task<Frente?> GetByCodigoAsync(string codigo);
        Task<Frente> CreateAsync(Frente frente);
        Task<Frente> UpdateAsync(Frente frente);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByCodigoAsync(string codigo, int? excludeId = null);
    }
}
