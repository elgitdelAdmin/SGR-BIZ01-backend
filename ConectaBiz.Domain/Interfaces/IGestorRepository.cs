using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Interfaces
{
    public interface IGestorRepository
    {
        Task<IEnumerable<Gestor>> GetAllAsync();
        Task<IEnumerable<Gestor>> GetByIdSocio(int idSocio);
        Task<Gestor?> GetByIdAsync(int id);
        Task<Gestor?> GetByIdUserAsync(int iduser);
        Task<Gestor?> GetByPersonaIdAsync(int personaId);
        Task<Gestor> CreateAsync(Gestor gestor);
        Task<Gestor> UpdateAsync(Gestor gestor);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByIdAsync(int id);
        Task<bool> ExistsByPersonaIdAsync(int personaId, int? excludeId = null);
    }

}
