using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Interfaces
{
    public interface ITicketHistorialRepository
    {
        Task<IEnumerable<TicketHistorialEstado>> GetByTicketIdAsync(int idTicket);
        Task<TicketHistorialEstado> CreateAsync(TicketHistorialEstado historial);
        Task<TicketHistorialEstado?> GetLastByTicketIdAsync(int idTicket);
        Task<bool> DeleteAsync(int id);
    }
}
