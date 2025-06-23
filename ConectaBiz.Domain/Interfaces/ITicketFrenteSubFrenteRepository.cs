using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Interfaces
{
    public interface ITicketFrenteSubFrenteRepository
    {
        Task<IEnumerable<TicketFrenteSubFrente>> GetByTicketIdAsync(int idTicket);
        Task<IEnumerable<TicketFrenteSubFrente>> GetActivosByTicketIdAsync(int idTicket);
        Task<TicketFrenteSubFrente> CreateAsync(TicketFrenteSubFrente frenteSubFrente);
        Task<TicketFrenteSubFrente> UpdateAsync(TicketFrenteSubFrente frenteSubFrente);
        Task<bool> DeactivateAllByTicketIdAsync(int idTicket, string usuarioModificacion);
        Task<bool> DeleteAsync(int id);
    }
}
