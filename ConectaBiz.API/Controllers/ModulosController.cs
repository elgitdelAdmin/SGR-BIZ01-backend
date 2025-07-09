using ConectaBiz.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> GetAllModulos()
        {
            var modulos = await _moduloService.GetAllModulosAsync();
            return Ok(modulos);
        }

        [HttpGet("por-rol/{idRol}")]
        public async Task<IActionResult> GetModulosPorRol(int idRol)
        {
            var permisos = await _moduloService.GetModulosByRolAsync(idRol);
            return Ok(new { menu = permisos });
        }
    }

}
