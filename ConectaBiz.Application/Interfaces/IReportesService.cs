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
    }


}
