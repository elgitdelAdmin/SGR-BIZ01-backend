using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConectaBiz.API.Controllers
{
    [ApiController]
    [Route("api/reportes")]
    public class ReportesController : ControllerBase
    {
        private readonly IReportesService _service;

        public ReportesController(IReportesService service)
        {
            _service = service;
        }

        [HttpPost("ReportesPorCorreo")]
        public async Task<IActionResult> Enviar3Excel([FromBody] Enviar3ReportesExcelRequest req)
        {
            if (req == null)
                return BadRequest("Request inválido.");

            if (req.Emails == null || req.Emails.Count == 0)
                return BadRequest("Debe enviar al menos un correo en Emails.");

            await _service.Enviar3ExcelsAsync(
                req.FechaDesdeAutorizados,
                req.FechaDesdeNoCerrados,
                req.Emails
            );

            return Ok(new
            {
                ok = true,
                message = "Correo enviado con 3 Excel adjuntos.",
                fechaDesdeAutorizados = req.FechaDesdeAutorizados.ToString("yyyy-MM-dd"),
                fechaDesdeNoCerrados = req.FechaDesdeNoCerrados.ToString("yyyy-MM-dd"),
                destinatarios = req.Emails.Distinct().ToList()
            });
        }

        [HttpPost("ConsultarDetalle")]
        public async Task<IActionResult> ConsultarDetalle([FromBody] FiltrosReporteRequest filtros)
        {
            if (filtros == null)
                return BadRequest("Filtros requeridos.");

            var data = await _service.ConsultarDetalleReporteAsync(filtros);
            return Ok(data);
        }

        [HttpGet("DashboardTicketsConsultor")]
        public async Task<IActionResult> DashboardTicketsConsultor()
        {
            var data = await _service.ConsultarDashboardTicketsConsultorAsync();
            return Ok(data);
        }

        [HttpPost("GenerarExcel")]
        public async Task<IActionResult> GenerarExcel([FromBody] FiltrosReporteRequest filtros)
        {
            if (filtros == null)
                return BadRequest("Filtros requeridos.");

            var excelBytes = await _service.GenerarReporteExcelAsync(filtros);
            var fileName = $"Reporte_{filtros.CodigoReporte ?? "General"}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            
            return File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }

        /// <summary>
        /// TEMPORAL: Método GET simple para descargar Excel mientras se construye el frontend.
        /// Solo requiere fechas e idPreporte. Borrar cuando el frontend esté listo.
        /// </summary>
        [HttpGet("GenerarExcelSimple")]
        public async Task<IActionResult> GenerarExcelSimple(
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin,
            [FromQuery] int idPreporte)
        {
            if (!fechaInicio.HasValue || !fechaFin.HasValue)
                return BadRequest("FechaInicio y FechaFin son requeridas.");

            if (idPreporte <= 0)
                return BadRequest("IdPreporte debe ser mayor a 0.");

            // Construir el objeto de filtros con null en todo excepto las fechas y el id
            var filtros = new FiltrosReporteRequest
            {
                IdTipoReporte = idPreporte,
                CodigoReporte = null,
                IdEmpresas = null,
                IdTickets = null,
                IdTiposTicket = null,
                IdSubtiposTicket = null,
                IdEstadosTicket = null,
                IdConsultores = null,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                IdSocio = null
            };

            var excelBytes = await _service.GenerarReporteExcelAsync(filtros);
            
            // Obtener el nombre del reporte desde la capa de lógica
            var nombreReporte = await _service.ObtenerNombreReporteAsync(idPreporte);
            var fileName = $"{nombreReporte}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            
            return File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }
    }
}
