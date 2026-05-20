using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Interfaces
{
    public interface IReportesRepository
    {
        Task<IEnumerable<IDictionary<string, object>>> GetAutorizadosGestorCuentaAsync(DateTime fecha);
        Task<IEnumerable<IDictionary<string, object>>> GetNoCerradosAsync(DateTime fecha);
        Task<IEnumerable<IDictionary<string, object>>> GetDetalleTareasConsultorAsync();
        Task<IEnumerable<IDictionary<string, object>>> GetDashboardTicketsConsultorAsync();
        
        Task<IEnumerable<IDictionary<string, object>>> ObtenerDatosReporteAsync(
            int? idTipoReporte,
            string? codigoReporte,
            List<int>? idEmpresas,
            List<int>? idTickets,
            List<int>? idTiposTicket,
            List<int>? idSubtiposTicket,
            List<int>? idEstadosTicket,
            List<int>? idConsultores,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? idUser,
            string? codRol
        );

        Task<IEnumerable<IDictionary<string, object>>> EjecutarReporteDinamicoAsync(string sql, object parameters);
    }
}
