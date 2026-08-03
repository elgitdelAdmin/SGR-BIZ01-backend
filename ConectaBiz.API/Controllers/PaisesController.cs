using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConectaBiz.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaisesController : ControllerBase
    {
        private readonly IPaisService _paisService;

        public PaisesController(IPaisService paisService)
        {
            _paisService = paisService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PaisDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PaisDto>>> GetPaises([FromQuery] bool? soloActivos = null)
        {
            var paises = soloActivos == true
                ? await _paisService.GetActiveAsync()
                : await _paisService.GetAllAsync();

            return Ok(paises);
        }
    }
}
