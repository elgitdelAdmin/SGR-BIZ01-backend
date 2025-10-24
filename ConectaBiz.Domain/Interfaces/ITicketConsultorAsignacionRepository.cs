using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Interfaces
{
    public interface ITicketConsultorAsignacionRepository
    {
        Task<IEnumerable<TicketConsultorAsignacion>> GetByTicketIdAsync(int idTicket);
        Task<IEnumerable<TicketConsultorAsignacion>> GetActivosByTicketIdAsync(int idTicket);
        Task<TicketConsultorAsignacion> CreateAsync(TicketConsultorAsignacion asignacion);
        Task<IEnumerable<TicketConsultorAsignacion>> CreateRangeAsync(List<TicketConsultorAsignacion> asignaciones);
        Task<IEnumerable<DetalleTareasConsultor>> CreateTareasRangeAsync(List<DetalleTareasConsultor> detallesTareas);
        Task<TicketConsultorAsignacion> UpdateAsync(TicketConsultorAsignacion asignacion);
        Task<IEnumerable<TicketConsultorAsignacion>> UpdateRangeAsync(List<TicketConsultorAsignacion> asignaciones);
        Task<IEnumerable<DetalleTareasConsultor>> UpdateTareasRangeAsync(List<DetalleTareasConsultor> detallesTareas);
        Task<bool> DeactivateAllByTicketIdAsync(int idTicket, string usuarioDesasignacion);
        Task<bool> DeleteAsync(int id);
    }
}
