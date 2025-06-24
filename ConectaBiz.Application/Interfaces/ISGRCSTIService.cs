using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Interfaces
{
    public interface ISGRCSTIService
    {
        Task MigracionEmpresa();
        Task<IEnumerable<dynamic>> MigracionRequerimientos();
    }
}
