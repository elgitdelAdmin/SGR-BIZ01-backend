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
    }
}
