using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Interfaces
{
    public interface IConsultorRepository
    {
        Task<IEnumerable<Consultor>> GetAllAsync();
        Task<Consultor> GetByIdAsync(int id);
        Task<Consultor> GetByIdPersonaAsync(int idPersona);
        Task<IEnumerable<Consultor>> GetByIdSocioAsync(int idSocio);
        Task<IEnumerable<Consultor>> GetByNumDocContribuyenteSocioAsync(string numDocContribuyente);
        Task<Consultor> GetByIdUserAsync(int iduser);
        Task<Consultor> CreateAsync(Consultor consultor);
        Task<Consultor> UpdateAsync(Consultor consultor);
        Task<Consultor> UpdateUserAsync(Consultor consultor);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByPersonaIdAsync(int personaId);
        Task<bool> ExistsByNumeroDocumentoAsync(string numeroDocumento);
    }
}
