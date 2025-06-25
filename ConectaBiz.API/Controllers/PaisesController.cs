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
        public async Task<ActionResult<IEnumerable<PaisDto>>> GetPaises([FromQuery] bool? soloActivos = null)
        {
            try
            {
                var paises = soloActivos == true
                    ? await _paisService.GetActiveAsync()
                    : await _paisService.GetAllAsync();

                return Ok(paises);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        //[HttpGet("{id}")]
        //public async Task<ActionResult<PaisDto>> GetPais(int id)
        //{
        //    try
        //    {
        //        var pais = await _paisService.GetByIdAsync(id);
        //        if (pais == null)
        //            return NotFound(new { message = $"No se encontró el país con ID {id}" });

        //        return Ok(pais);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        //    }
        //}

        //[HttpGet("codigo/{codigo}")]
        //public async Task<ActionResult<PaisDto>> GetPaisByCodigo(string codigo)
        //{
        //    try
        //    {
        //        var pais = await _paisService.GetByCodigoAsync(codigo);
        //        if (pais == null)
        //            return NotFound(new { message = $"No se encontró el país con código '{codigo}'" });

        //        return Ok(pais);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        //    }
        //}

        //[HttpPost]
        //public async Task<ActionResult<PaisDto>> CreatePais(CreatePaisDto createPaisDto)
        //{
        //    try
        //    {
        //        var paisCreado = await _paisService.CreateAsync(createPaisDto);
        //        return CreatedAtAction(nameof(GetPais), new { id = paisCreado.Id }, paisCreado);
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        //    }
        //}

        //[HttpPut("{id}")]
        //public async Task<ActionResult<PaisDto>> UpdatePais(int id, UpdatePaisDto updatePaisDto)
        //{
        //    try
        //    {
        //        var paisActualizado = await _paisService.UpdateAsync(id, updatePaisDto);
        //        return Ok(paisActualizado);
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        return NotFound(new { message = ex.Message });
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        //    }
        //}

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeletePais(int id, [FromQuery] string? usuarioModificacion = null)
        //{
        //    try
        //    {
        //        await _paisService.DeleteAsync(id, usuarioModificacion);
        //        return Ok(new { message = "País desactivado exitosamente" });
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        return NotFound(new { message = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        //    }
        //}
    }
}
