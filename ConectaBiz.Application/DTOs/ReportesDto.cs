using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.DTOs
{
    public class Enviar3ReportesExcelRequest
    {
        public DateTime FechaDesdeAutorizados { get; set; } // Ej: 2026-01-01
        public DateTime FechaDesdeNoCerrados { get; set; }  // Ej: 2025-01-01
        public List<string> Emails { get; set; } = new();
    }
}
