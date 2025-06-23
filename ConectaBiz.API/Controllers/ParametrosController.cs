using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConectaBiz.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParametrosController : ControllerBase
    {
        private readonly IParametroService _parametroService;

        public ParametrosController(IParametroService parametroService)
        {
            _parametroService = parametroService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ParametroDto>>> GetAll()
        {
            var parametros = await _parametroService.GetAllAsync();
            return Ok(parametros);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ParametroDto>> GetById(int id)
        {
            var parametro = await _parametroService.GetByIdAsync(id);
            if (parametro == null)
                return NotFound();

            return Ok(parametro);
        }

        [HttpGet("tipo/{tipoParametro}")]
        public async Task<ActionResult<IEnumerable<ParametroDto>>> GetByTipo(string tipoParametro)
        {
            var parametros = await _parametroService.GetByTipoParametroAsync(tipoParametro);
            return Ok(parametros);
        }

        [HttpGet("activos")]
        public async Task<ActionResult<IEnumerable<ParametroDto>>> GetActivos()
        {
            var parametros = await _parametroService.GetActivosAsync();
            return Ok(parametros);
        }

        [HttpGet("tipo/{tipoParametro}/codigo/{codigo}")]
        public async Task<ActionResult<ParametroDto>> GetByCodigo(string tipoParametro, string codigo)
        {
            var parametro = await _parametroService.GetByCodigoAsync(tipoParametro, codigo);
            if (parametro == null)
                return NotFound();

            return Ok(parametro);
        }

        [HttpPost]
        public async Task<ActionResult<ParametroDto>> Create(CreateParametroDto createDto)
        {
            try
            {
                var parametro = await _parametroService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = parametro.Id }, parametro);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ParametroDto>> Update(int id, UpdateParametroDto updateDto)
        {
            try
            {
                var parametro = await _parametroService.UpdateAsync(id, updateDto);
                return Ok(parametro);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _parametroService.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}