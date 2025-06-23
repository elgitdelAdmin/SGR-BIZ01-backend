using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConectaBiz.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GestorController : ControllerBase
    {
        private readonly IGestorService _gestorService;

        public GestorController(IGestorService gestorService)
        {
            _gestorService = gestorService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GestorDto>>> GetAll()
        {
            try
            {
                var gestores = await _gestorService.GetAllAsync();
                return Ok(gestores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GestorDto>> GetById(int id)
        {
            try
            {
                var gestor = await _gestorService.GetByIdAsync(id);
                if (gestor == null)
                {
                    return NotFound(new { message = $"No se encontró el gestor con ID {id}" });
                }
                return Ok(gestor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<GestorDto>> Create([FromBody] CreateGestorDto createGestorDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var gestor = await _gestorService.CreateAsync(createGestorDto);
                return CreatedAtAction(nameof(GetById), new { id = gestor.Id }, gestor);
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

        [HttpPut("{id}")]
        public async Task<ActionResult<GestorDto>> Update(int id, [FromBody] UpdateGestorDto updateGestorDto)
        {
            try
            {
                if (id != updateGestorDto.Id)
                {
                    return BadRequest(new { message = "El ID de la URL no coincide con el ID del objeto" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var gestor = await _gestorService.UpdateAsync(updateGestorDto);
                return Ok(gestor);
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

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _gestorService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"No se encontró el gestor con ID {id}" });
                }
                return NoContent();
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
    }
}