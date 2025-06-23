using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Interfaces
{
    public interface IPaisRepository
    {
        Task<IEnumerable<Pais>> GetAllAsync();
        Task<IEnumerable<Pais>> GetActiveAsync();
        Task<Pais?> GetByIdAsync(int id);
        Task<Pais?> GetByCodigoAsync(string codigo);
        Task<Pais> CreateAsync(Pais pais);
        Task<Pais> UpdateAsync(Pais pais);
        Task<bool> DeleteAsync(int id, string? usuarioModificacion = null);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByCodigoAsync(string codigo, int? excludeId = null);
    }
}
