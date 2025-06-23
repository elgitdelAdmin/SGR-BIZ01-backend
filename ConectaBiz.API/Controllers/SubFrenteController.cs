using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConectaBiz.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubFrenteController : ControllerBase
    {
        private readonly ISubFrenteService _subFrenteService;

        public SubFrenteController(ISubFrenteService subFrenteService)
        {
            _subFrenteService = subFrenteService;
        }

        /// <summary>
        /// Obtener todos los sub-frentes
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubFrenteDto>>> GetAll()
        {
            try
            {
                var subFrente = await _subFrenteService.GetAllAsync();
                return Ok(subFrente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener solo los sub-frentes activos
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<SubFrenteDto>>> GetActive()
        {
            try
            {
                var subFrente = await _subFrenteService.GetActiveAsync();
                return Ok(subFrente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener sub-frentes por ID del frente padre
        /// </summary>
        [HttpGet("by-frente/{frenteId}")]
        public async Task<ActionResult<IEnumerable<SubFrenteDto>>> GetByFrenteId(int frenteId)
        {
            try
            {
                var subFrente = await _subFrenteService.GetByFrenteIdAsync(frenteId);
                return Ok(subFrente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener un sub-frente por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<SubFrenteDto>> GetById(int id)
        {
            try
            {
                var subFrente = await _subFrenteService.GetByIdAsync(id);
                if (subFrente == null)
                    return NotFound(new { message = $"No se encontró el sub-frente con ID {id}" });

                return Ok(subFrente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener un sub-frente por ID incluyendo su frente padre
        /// </summary>
        [HttpGet("{id}/with-frente")]
        public async Task<ActionResult<SubFrenteDto>> GetByIdWithFrente(int id)
        {
            try
            {
                var subFrente = await _subFrenteService.GetByIdWithFrenteAsync(id);
                if (subFrente == null)
                    return NotFound(new { message = $"No se encontró el sub-frente con ID {id}" });

                return Ok(subFrente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Crear un nuevo sub-frente
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<SubFrenteDto>> Create([FromBody] SubFrenteDto subFrenteDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var createdSubFrente = await _subFrenteService.CreateAsync(subFrenteDto);
                return CreatedAtAction(nameof(GetById), new { id = createdSubFrente.Id }, createdSubFrente);
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
        /// Actualizar un sub-frente existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<SubFrenteDto>> Update(int id, [FromBody] SubFrenteDto subFrenteDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updatedSubFrente = await _subFrenteService.UpdateAsync(id, subFrenteDto);
                return Ok(updatedSubFrente);
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
        /// Eliminar un sub-frente
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _subFrenteService.DeleteAsync(id);
                if (!deleted)
                    return NotFound(new { message = $"No se encontró el sub-frente con ID {id}" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Verificar si existe un sub-frente
        /// </summary>
        [HttpHead("{id}")]
        public async Task<ActionResult> Exists(int id)
        {
            try
            {
                var exists = await _subFrenteService.ExistsAsync(id);
                return exists ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }
    }
}
