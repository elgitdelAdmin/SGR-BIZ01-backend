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
    }
}
