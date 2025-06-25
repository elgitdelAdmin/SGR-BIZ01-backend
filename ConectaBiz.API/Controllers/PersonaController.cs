using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConectaBiz.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonaController : ControllerBase
    {
        private readonly IPersonaService _personaService;
        private readonly ILogger<PersonaController> _logger;

        public PersonaController(IPersonaService personaService, ILogger<PersonaController> logger)
        {
            _personaService = personaService;
            _logger = logger;
        }

        // GET: api/Persona
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PersonaDto>>> GetAllPersonas()
        {
            try
            {
                var personas = await _personaService.GetAllAsync();
                return Ok(personas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las personas");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        //// GET: api/Persona/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<PersonaDto>> GetPersonaById(int id)
        //{
        //    try
        //    {
        //        var persona = await _personaService.GetByIdAsync(id);
        //        if (persona == null)
        //            return NotFound($"Persona con ID {id} no encontrada");

        //        return Ok(persona);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error al obtener la persona con ID {id}");
        //        return StatusCode(500, "Error interno del servidor");
        //    }
        //}

        //// POST: api/Persona
        //[HttpPost]
        //public async Task<ActionResult<PersonaDto>> CreatePersona([FromBody] PersonaDto personaDto)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //            return BadRequest(ModelState);

        //        var createdPersona = await _personaService.CreateAsync(personaDto);
        //        return CreatedAtAction(nameof(GetPersonaById), new { id = createdPersona.Id }, createdPersona);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al crear una persona");
        //        return StatusCode(500, "Error interno del servidor");
        //    }
        //}

        //// PUT: api/Persona/5
        //[HttpPut("{id}")]
        //public async Task<ActionResult<PersonaDto>> UpdatePersona(int id, [FromBody] PersonaDto personaDto)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //            return BadRequest(ModelState);

        //        var updatedPersona = await _personaService.UpdateAsync(id, personaDto);
        //        if (updatedPersona == null)
        //            return NotFound($"Persona con ID {id} no encontrada");

        //        return Ok(updatedPersona);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error al actualizar la persona con ID {id}");
        //        return StatusCode(500, "Error interno del servidor");
        //    }
        //}

        //// DELETE: api/Persona/5
        //[HttpDelete("{id}")]
        //public async Task<ActionResult> DeletePersona(int id)
        //{
        //    try
        //    {
        //        var result = await _personaService.DeleteAsync(id);
        //        if (!result)
        //            return NotFound($"Persona con ID {id} no encontrada");

        //        return NoContent();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error al eliminar la persona con ID {id}");
        //        return StatusCode(500, "Error interno del servidor");
        //    }
        //}
    }
}
