using ConectaBiz.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ConectaBiz.Application.DTOs;

namespace ConectaBiz.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModulosController : ControllerBase
    {
        private readonly IModuloService _moduloService;

        public ModulosController(IModuloService moduloService)
        {
            _moduloService = moduloService;
        }

        [HttpGet("all")]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllModulos()
        {
            var modulos = await _moduloService.GetAllModulosAsync();
            return Ok(modulos);
        }

        [HttpGet("por-rol/{idRol}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetModulosPorRol(int idRol)
        {
            var permisos = await _moduloService.GetModulosByRolAsync(idRol);
            return Ok(new { menu = permisos });
        }
    }
}
