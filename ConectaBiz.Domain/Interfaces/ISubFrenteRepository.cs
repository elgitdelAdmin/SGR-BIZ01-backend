using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Interfaces
{
    public interface ISubFrenteRepository
    {
        Task<IEnumerable<SubFrente>> GetAllAsync();
        Task<IEnumerable<SubFrente>> GetActiveAsync();
        Task<IEnumerable<SubFrente>> GetByFrenteIdAsync(int frenteId);
        Task<SubFrente?> GetByIdAsync(int id);
        Task<SubFrente?> GetByIdWithFrenteAsync(int id);
        Task<SubFrente?> GetByCodigoAsync(string codigo);
        Task<SubFrente> CreateAsync(SubFrente subFrente);
        Task<SubFrente> UpdateAsync(SubFrente subFrente);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByCodigoAsync(string codigo, int? excludeId = null);
    }
}
