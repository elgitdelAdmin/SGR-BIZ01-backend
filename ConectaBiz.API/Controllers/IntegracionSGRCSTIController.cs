using ConectaBiz.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConectaBiz.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IntegracionSGRCSTIController : ControllerBase
    {
        private readonly ISGRCSTIService _sgrcstiService;
        public IntegracionSGRCSTIController(ISGRCSTIService sGRCSTIService)
        {
            _sgrcstiService = sGRCSTIService;
        }
        [HttpGet("MigracionEmpresa")]
        public async Task<IActionResult> MigracionEmpresa()
        {
            try
            {
                await _sgrcstiService.MigracionEmpresa();
                return Ok(new { message = "Migración de empresas completada exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }
    }
}
