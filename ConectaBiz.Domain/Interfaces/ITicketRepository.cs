using ConectaBiz.Domain.Entities;

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
        Task<IEnumerable<Ticket>> GetByGestorAsync(int idGestor, int? idSocio = null);
        Task<IEnumerable<Ticket>> GetByGestorConsultoriaAsync(int idGestor, int? idSocio = null);
        Task<IEnumerable<Ticket>> GetByConsultorAsync(int idConsultor, int? idSocio = null);
        Task<Ticket> CreateAsync(Ticket ticket);
        Task<List<Ticket>> CreateRangeAsync(List<Ticket> tickets);
        Task<Ticket> UpdateAsync(Ticket ticket);
        //Task<IEnumerable<Ticket>> UpdateRangeAsync(IEnumerable<Ticket> tickets);
        Task<bool> DeleteAsync(int id);
        //Task<bool> ExistsAsync(string codTicket, int? excludeId = null);
        //Task<IEnumerable<Ticket>> GetTicketsWithFiltersAsync(int? idEmpresa = null, int? idEstado = null, bool? urgente = null);
        //Task<IEnumerable<TicketConsultorAsignacion>> GetConsultorAsignacionesActivasByTicketIdAsync(int idTicket);
        //Task<IEnumerable<TicketFrenteSubFrente>> GetFrenteSubFrentesActivosByTicketIdAsync(int idTicket);
        Task<Ticket?> GetByCodReqSgrCstiAsync(string codReqSgrCsti);

        // ── Métodos IQueryable para paginación server-side ──
        IQueryable<Ticket> GetQueryableByGestor(int idGestor, int? idSocio = null);
        IQueryable<Ticket> GetQueryableByGestorConsultoria(int idGestor, int? idSocio = null);
        IQueryable<Ticket> GetQueryableByConsultor(int idConsultor, int? idSocio = null);
        IQueryable<Ticket> GetQueryableByEmpresa(int idEmpresa);
        IQueryable<Ticket> GetQueryableBySocio(int idSocio);
        IQueryable<Ticket> GetQueryableAll();
        Task<List<string>> GetAllCodTicketInternosAsync();
        Task<Ticket?> GetByCualquierCodigoAsync(string codigo);
        Task<int> GetMaxIdAsync();
    }
}
