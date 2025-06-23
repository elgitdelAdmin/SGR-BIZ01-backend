using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Interfaces
{
    public interface IParametroRepository
    {
        Task<IEnumerable<Parametro>> GetAllAsync();
        Task<Parametro?> GetByIdAsync(int id);
        Task<IEnumerable<Parametro>> GetByTipoParametroAsync(string tipoParametro);
        Task<IEnumerable<Parametro>> GetActivosAsync();
        Task<Parametro?> GetByCodigoAsync(string tipoParametro, string codigo);
        Task<Parametro> CreateAsync(Parametro parametro);
        Task<Parametro> UpdateAsync(Parametro parametro);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string tipoParametro, string codigo, int? excludeId = null);
    }
}
