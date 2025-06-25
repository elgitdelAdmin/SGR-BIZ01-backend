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
        /// <summary>
        /// Obtiene todos los consultores activos
        /// </summary>
        Task<IEnumerable<Consultor>> GetAllAsync();

        /// <summary>
        /// Obtiene un consultor por su ID
        /// </summary>
        Task<Consultor> GetByIdAsync(int id);
        Task<Consultor> GetByIdUserAsync(int iduser);

        /// <summary>
        /// Crea un nuevo consultor
        /// </summary>
        Task<Consultor> CreateAsync(Consultor consultor);

        /// <summary>
        /// Actualiza un consultor existente
        /// </summary>
        Task<Consultor> UpdateAsync(Consultor consultor);

        /// <summary>
        /// Elimina lógicamente un consultor por su ID
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Verifica si existe un consultor con el ID especificado
        /// </summary>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Verifica si existe un consultor asociado a una persona
        /// </summary>
        Task<bool> ExistsByPersonaIdAsync(int personaId);
        Task<bool> ExistsByNumeroDocumentoAsync(string numeroDocumento);
    }
}
