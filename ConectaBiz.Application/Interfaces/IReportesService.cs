using ConectaBiz.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Interfaces
{
    public interface IReportesService
    {
        Task Enviar3ExcelsAsync(
            DateTime fechaDesdeAutorizados,
            DateTime fechaDesdeNoCerrados,
            List<string> emails
        );

        Task<IEnumerable<IDictionary<string, object>>> ConsultarDetalleReporteAsync(FiltrosReporteRequest filtros);
        
        Task<byte[]> GenerarReporteExcelAsync(FiltrosReporteRequest filtros);

        Task<string> ObtenerNombreReporteAsync(int idTipoReporte);
    }


}
