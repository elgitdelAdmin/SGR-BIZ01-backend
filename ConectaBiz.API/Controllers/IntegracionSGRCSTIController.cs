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
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MigracionEmpresa()
        {
            await _sgrcstiService.MigracionEmpresa();
            return Ok(new { message = "Migración de empresas completada exitosamente." });
        }

        [HttpGet("MigracionRequerimientos")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MigracionRequerimientos()
        {
            var resultado = await _sgrcstiService.MigracionRequerimientos();
            return Ok(resultado);
        }
    }
}
