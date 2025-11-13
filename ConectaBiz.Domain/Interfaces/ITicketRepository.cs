using ConectaBiz.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Interfaces
{
    public interface ITicketRepository
    {
        Task<IEnumerable<Ticket>> GetAllAsync();
        Task<Ticket?> GetByIdAsync(int id);
        Task<Ticket?> GetByIdWithRelationsAsync(int id);
        Task<Ticket?> GetByCodTicketAsync(string codTicket);
        Task<IEnumerable<Ticket>> GetByEmpresaAsync(int idEmpresa);
        Task<IEnumerable<Ticket>> GetBySocioAsync(int idSocio);
        Task<IEnumerable<Ticket>> GetByIdSocioNumContribuyenteEmpAsync(int idSocio, string numContribuyenteEmp);
        Task<IEnumerable<Ticket>> GetByNumContribuyenteSocioEmpAsync(string numContribuyenteSocio, string numContribuyenteEmp);
        Task<IEnumerable<Ticket>> GetByEstadoAsync(int idEstado);
        Task<IEnumerable<Ticket>> GetByGestorAsync(int idGestor);
        Task<IEnumerable<Ticket>> GetByGestorConsultoriaAsync(int idGestor);
        Task<IEnumerable<Ticket>> GetByConsultorAsync(int idConsultor);
        Task<Ticket> CreateAsync(Ticket ticket);
        Task<List<Ticket>> CreateRangeAsync(List<Ticket> tickets);
        Task<Ticket> UpdateAsync(Ticket ticket);
        Task<IEnumerable<Ticket>> UpdateRangeAsync(IEnumerable<Ticket> tickets);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string codTicket, int? excludeId = null);
        Task<IEnumerable<Ticket>> GetTicketsWithFiltersAsync(int? idEmpresa = null, int? idEstado = null, bool? urgente = null);
        Task<IEnumerable<TicketConsultorAsignacion>> GetConsultorAsignacionesActivasByTicketIdAsync(int idTicket);
        Task<IEnumerable<TicketFrenteSubFrente>> GetFrenteSubFrentesActivosByTicketIdAsync(int idTicket);
        Task<Ticket?> GetByCodReqSgrCstiAsync(string codReqSgrCsti);
    }
}
