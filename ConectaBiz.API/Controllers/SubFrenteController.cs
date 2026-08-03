using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Exceptions;
using ConectaBiz.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConectaBiz.API.Controllers
{
    // PATRÓN IMPLEMENTADO: Primary Adapter (Adaptador de Entrada) / Controlador Limpio
    // 
    // Gracias a la delegación del manejo de errores al Middleware global, 
    // este controlador (y los demás) queda completamente libre de bloques try-catch.
    // Su única responsabilidad ahora sigue el Principio de Responsabilidad Única (SRP):
    // 1. Recibir la petición HTTP.
    // 2. Delegar la lógica de negocio a la capa de Aplicación (Servicio).
    // 3. Retornar el resultado exitoso con el código HTTP correspondiente (200, 201, 204).
    [ApiController]
    [Route("api/[controller]")]
    public class SubFrenteController : ControllerBase
    {
        private readonly ISubFrenteService _subFrenteService;

        public SubFrenteController(ISubFrenteService subFrenteService)
        {
            _subFrenteService = subFrenteService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SubFrenteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<SubFrenteDto>>> GetAll()
        {
            var subFrente = await _subFrenteService.GetAllAsync();
            return Ok(subFrente);
        }

        [HttpGet("active")]
        [ProducesResponseType(typeof(IEnumerable<SubFrenteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<SubFrenteDto>>> GetActive()
        {
            var subFrente = await _subFrenteService.GetActiveAsync();
            return Ok(subFrente);
        }

        [HttpGet("by-frente/{frenteId}")]
        [ProducesResponseType(typeof(IEnumerable<SubFrenteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<SubFrenteDto>>> GetByFrenteId(int frenteId)
        {
            var subFrente = await _subFrenteService.GetByFrenteIdAsync(frenteId);
            return Ok(subFrente);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SubFrenteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SubFrenteDto>> GetById(int id)
        {
            var subFrente = await _subFrenteService.GetByIdAsync(id);
            if (subFrente == null)
                throw new KeyNotFoundException($"No se encontró el sub-frente con ID {id}");

            return Ok(subFrente);
        }

        [HttpPost]
        [ProducesResponseType(typeof(SubFrenteDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SubFrenteDto>> Create([FromBody] CreateSubFrenteDto createSubFrenteDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdSubFrente = await _subFrenteService.CreateAsync(createSubFrenteDto);
            return CreatedAtAction(nameof(GetById), new { id = createdSubFrente.Id }, createdSubFrente);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(SubFrenteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SubFrenteDto>> Update(int id, [FromBody] UpdateSubFrenteDto updateSubFrenteDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedSubFrente = await _subFrenteService.UpdateAsync(id, updateSubFrenteDto);
            return Ok(updatedSubFrente);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(int id)
        {
            var deleted = await _subFrenteService.DeleteAsync(id);
            if (!deleted)
                throw new KeyNotFoundException($"No se encontró el sub-frente con ID {id}");

            return Ok(new { message = "Sub-frente desactivado exitosamente" });
        }

        [HttpGet("{id}/consultores-asociados")]
        [ProducesResponseType(typeof(IEnumerable<ConsultorAsociadoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ConsultorAsociadoDto>>> GetConsultoresAsociados(int id)
        {
            var consultores = await _subFrenteService.GetConsultoresAsociadosBySubFrenteIdAsync(id);
            return Ok(consultores);
        }
    }
}
