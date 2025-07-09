using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Interfaces
{
    public interface IPersonaRepository
    {
        Task<IEnumerable<Persona>> GetAllAsync();
        Task<Persona> GetByIdAsync(int id);
        Task<Persona> GetByNumeroDocumentoAsync(string numeroDocumento);
        Task<Persona> CreateAsync(Persona persona);
        Task<Persona> UpdateAsync(Persona persona);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByTipoNumDocumentoAsync(int tipoDocumento, string numeroDocumento, int? excludeId = null);
        Task<Persona?> GetByTipoNumDocumentoAsync(int tipoDocumento, string numeroDocumento);
        Task<Persona> GetByResponsableTipoNumDocumentoAsync(int tipoDocumento, string numeroDocumento, string codigoRol);

    }
}
