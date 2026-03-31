using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Exceptions;
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
        /// Crear un nuevo sub-frente
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<SubFrenteDto>> Create([FromBody] CreateSubFrenteDto createSubFrenteDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var createdSubFrente = await _subFrenteService.CreateAsync(createSubFrenteDto);
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
        public async Task<ActionResult<SubFrenteDto>> Update(int id, [FromBody] UpdateSubFrenteDto updateSubFrenteDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updatedSubFrente = await _subFrenteService.UpdateAsync(id, updateSubFrenteDto);
                return Ok(updatedSubFrente);
            }
            catch (ConsultoresAsociadosException ex)
            {
                return StatusCode(409, new { message = ex.Message, consultores = ex.Consultores });
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
        /// Eliminar un sub-frente (eliminación lógica)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _subFrenteService.DeleteAsync(id);
                if (!deleted)
                    return NotFound(new { message = $"No se encontró el sub-frente con ID {id}" });

                return Ok(new { message = "Sub-frente desactivado exitosamente" });
            }
            catch (ConsultoresAsociadosException ex)
            {
                return StatusCode(409, new { message = ex.Message, consultores = ex.Consultores });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener consultores asociados a un sub-frente
        /// </summary>
        [HttpGet("{id}/consultores-asociados")]
        public async Task<ActionResult<IEnumerable<ConsultorAsociadoDto>>> GetConsultoresAsociados(int id)
        {
            try
            {
                var consultores = await _subFrenteService.GetConsultoresAsociadosBySubFrenteIdAsync(id);
                return Ok(consultores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }
    }
}
