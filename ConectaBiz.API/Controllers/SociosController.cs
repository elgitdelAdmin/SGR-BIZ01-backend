using Microsoft.AspNetCore.Mvc;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;

namespace ConectaBiz.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SociosController : ControllerBase
    {
        private readonly ISocioService _socioService;

        public SociosController(ISocioService socioService)
        {
            _socioService = socioService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<SocioDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<SocioDto>>> ListarTodos()
        {
            var socios = await _socioService.ListarTodosAsync();
            return Ok(socios);
        }

        /// <summary>
        /// Obtiene un socio por su ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SocioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SocioDto>> ObtenerPorId(int id)
        {
            if (id <= 0)
            {
                return BadRequest("El ID debe ser mayor a 0");
            }

            var socio = await _socioService.ObtenerPorIdAsync(id);
            if (socio == null)
            {
                throw new KeyNotFoundException($"No se encontró el socio con ID: {id}");
            }

            return Ok(socio);
        }

        [HttpPost]
        [ProducesResponseType(typeof(SocioDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SocioDto>> Crear([FromBody] SocioCreateDto socioCreateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var socioCreado = await _socioService.CrearAsync(socioCreateDto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = socioCreado.Id }, socioCreado);
        }

        /// <summary>
        /// Actualiza un socio existente
        /// NOTA: El NumDocContribuyente NO se puede modificar
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(SocioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SocioDto>> Actualizar(int id, [FromBody] SocioUpdateDto socioUpdateDto)
        {
            if (id <= 0)
            {
                return BadRequest("El ID debe ser mayor a 0");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var socioActualizado = await _socioService.ActualizarAsync(id, socioUpdateDto);
            return Ok(socioActualizado);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Eliminar(int id)
        {
            if (id <= 0)
            {
                return BadRequest("El ID debe ser mayor a 0");
            }

            var eliminado = await _socioService.EliminarAsync(id);
            if (!eliminado)
            {
                throw new KeyNotFoundException($"No se encontró el socio con ID: {id}");
            }

            return NoContent();
        }
    }
}