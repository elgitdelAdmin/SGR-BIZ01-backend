using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConectaBiz.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrenteController : ControllerBase
    {
        private readonly IFrenteService _frenteService;

        public FrenteController(IFrenteService frenteService)
        {
            _frenteService = frenteService;
        }

        /// <summary>
        /// Obtener todos los frentes
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FrenteDto>>> GetAll()
        {
            try
            {
                var frentes = await _frenteService.GetAllAsync();
                return Ok(frentes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener solo los frentes activos
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<FrenteDto>>> GetActive()
        {
            try
            {
                var frentes = await _frenteService.GetActiveAsync();
                return Ok(frentes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener un frente por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<FrenteDto>> GetById(int id)
        {
            try
            {
                var frente = await _frenteService.GetByIdAsync(id);
                if (frente == null)
                    return NotFound(new { message = $"No se encontró el frente con ID {id}" });

                return Ok(frente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener un frente por ID incluyendo sus sub-frentes
        /// </summary>
        [HttpGet("{id}/with-subfrentes")]
        public async Task<ActionResult<FrenteDto>> GetByIdWithSubFrente(int id)
        {
            try
            {
                var frente = await _frenteService.GetByIdWithSubFrentesAsync(id);
                if (frente == null)
                    return NotFound(new { message = $"No se encontró el frente con ID {id}" });

                return Ok(frente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Crear un nuevo frente
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<FrenteDto>> Create([FromBody] FrenteDto frenteDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var createdFrente = await _frenteService.CreateAsync(frenteDto);
                return CreatedAtAction(nameof(GetById), new { id = createdFrente.Id }, createdFrente);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualizar un frente existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<FrenteDto>> Update(int id, [FromBody] FrenteDto frenteDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updatedFrente = await _frenteService.UpdateAsync(id, frenteDto);
                return Ok(updatedFrente);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Eliminar un frente
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _frenteService.DeleteAsync(id);
                if (!deleted)
                    return NotFound(new { message = $"No se encontró el frente con ID {id}" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Verificar si existe un frente
        /// </summary>
        [HttpHead("{id}")]
        public async Task<ActionResult> Exists(int id)
        {
            try
            {
                var exists = await _frenteService.ExistsAsync(id);
                return exists ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }
    }
}
