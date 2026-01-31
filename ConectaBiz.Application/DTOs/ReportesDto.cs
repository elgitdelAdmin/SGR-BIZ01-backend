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

    public class FiltrosReporteRequest
    {
        public int IdTipoReporte { get; set; }
        public string CodigoReporte { get; set; }
        public List<int>? IdEmpresas { get; set; } = new();
        public List<int>? IdTickets { get; set; } = new();
        public List<int>? IdTiposTicket { get; set; } = new();
        public List<int>? IdSubtiposTicket { get; set; } = new();
        public List<int>? IdEstadosTicket { get; set; } = new();
        public List<int>? IdConsultores { get; set; } = new();
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? IdSocio { get; set; }
    }
}