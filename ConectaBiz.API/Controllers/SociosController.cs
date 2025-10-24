using Microsoft.AspNetCore.Mvc;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;

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
    public async Task<ActionResult<List<SocioDto>>> ListarTodos()
    {
        var socios = await _socioService.ListarTodosAsync();
        return Ok(socios);
    }
    /// <summary>
    /// Obtiene un socio por su ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SocioDto>> ObtenerPorId(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest("El ID debe ser mayor a 0");
            }

            var socio = await _socioService.ObtenerPorIdAsync(id);
            if (socio == null)
            {
                return NotFound($"No se encontró el socio con ID: {id}");
            }

            return Ok(socio);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<SocioDto>> Crear([FromBody] SocioCreateDto socioCreateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var socioCreado = await _socioService.CrearAsync(socioCreateDto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = socioCreado.Id }, socioCreado);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Actualiza un socio existente
    /// NOTA: El NumDocContribuyente NO se puede modificar
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<SocioDto>> Actualizar(int id, [FromBody] SocioUpdateDto socioUpdateDto)
    {
        try
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
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Eliminar(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest("El ID debe ser mayor a 0");
            }

            var eliminado = await _socioService.EliminarAsync(id);
            if (!eliminado)
            {
                return NotFound($"No se encontró el socio con ID: {id}");
            }

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}