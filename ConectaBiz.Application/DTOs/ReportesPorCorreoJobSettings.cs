using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.DTOs
{
    public class ReportesPorCorreoJobSettings
    {
        public string HoraEjecucion { get; set; } = "08:00";
        public DateTime FechaDesdeAutorizados { get; set; }
        public DateTime FechaDesdeNoCerrados { get; set; }
        public List<string> Emails { get; set; } = new();
    }
}
