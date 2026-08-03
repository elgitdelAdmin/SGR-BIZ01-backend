using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<FrenteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<FrenteDto>>> GetAll()
        {
            var frentes = await _frenteService.GetAllAsync();
            return Ok(frentes);
        }

        [HttpGet("active")]
        [ProducesResponseType(typeof(IEnumerable<FrenteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<FrenteDto>>> GetActive()
        {
            var frentes = await _frenteService.GetActiveAsync();
            return Ok(frentes);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(FrenteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<FrenteDto>> GetById(int id)
        {
            var frente = await _frenteService.GetByIdAsync(id);
            if (frente == null)
                throw new KeyNotFoundException($"No se encontró el frente con ID {id}");

            return Ok(frente);
        }

        [HttpGet("{id}/with-subfrentes")]
        [ProducesResponseType(typeof(FrenteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<FrenteDto>> GetByIdWithSubFrente(int id)
        {
            var frente = await _frenteService.GetByIdWithSubFrentesAsync(id);
            if (frente == null)
                throw new KeyNotFoundException($"No se encontró el frente con ID {id}");

            return Ok(frente);
        }

        [HttpPost]
        [ProducesResponseType(typeof(FrenteDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<FrenteDto>> Create([FromBody] CreateFrenteDto createFrenteDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdFrente = await _frenteService.CreateAsync(createFrenteDto);
            return CreatedAtAction(nameof(GetById), new { id = createdFrente.Id }, createdFrente);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(FrenteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<FrenteDto>> Update(int id, [FromBody] UpdateFrenteDto updateFrenteDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedFrente = await _frenteService.UpdateAsync(id, updateFrenteDto);
            return Ok(updatedFrente);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(int id)
        {
            var deleted = await _frenteService.DeleteAsync(id);
            if (!deleted)
                throw new KeyNotFoundException($"No se encontró el frente con ID {id}");

            return Ok(new { message = "Frente desactivado exitosamente" });
        }

        [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
        [HttpGet("{id}/consultores-asociados")]
        [ProducesResponseType(typeof(IEnumerable<ConsultorAsociadoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ConsultorAsociadoDto>>> GetConsultoresAsociados(int id)
        {
            var consultores = await _frenteService.GetConsultoresAsociadosByFrenteIdAsync(id);
            return Ok(consultores);
        }
    }
}
