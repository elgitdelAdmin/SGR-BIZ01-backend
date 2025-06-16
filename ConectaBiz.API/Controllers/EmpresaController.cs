using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConectaBiz.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmpresasController : ControllerBase
    {
        private readonly IEmpresaService _empresaService;

        public EmpresasController(IEmpresaService empresaService)
        {
            _empresaService = empresaService;
        }

        // GET: api/empresas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmpresaDto>>> GetAll([FromQuery] bool soloActivos = false)
        {
            try
            {
                var empresas = soloActivos
                    ? await _empresaService.GetAllActiveAsync()
                    : await _empresaService.GetAllAsync();

                return Ok(empresas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
            }
        }

        // GET: api/empresas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EmpresaDto>> GetById(int id)
        {
            try
            {
                var empresa = await _empresaService.GetByIdAsync(id);

                if (empresa == null)
                    return NotFound(new { message = $"No se encontró la empresa con ID {id}" });

                return Ok(empresa);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
            }
        }

        // GET: api/empresas/codigo/EMP001
        [HttpGet("codigo/{codigo}")]
        public async Task<ActionResult<EmpresaDto>> GetByCodigo(string codigo)
        {
            try
            {
                var empresa = await _empresaService.GetByCodigoAsync(codigo);

                if (empresa == null)
                    return NotFound(new { message = $"No se encontró la empresa con código {codigo}" });

                return Ok(empresa);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
            }
        }

        // GET: api/empresas/socio/5
        [HttpGet("socio/{idSocio}")]
        public async Task<ActionResult<IEnumerable<EmpresaDto>>> GetBySocio(int idSocio)
        {
            try
            {
                var empresas = await _empresaService.GetBySocioAsync(idSocio);
                return Ok(empresas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
            }
        }

        // GET: api/empresas/gestor/5
        [HttpGet("gestor/{idGestor}")]
        public async Task<ActionResult<IEnumerable<EmpresaDto>>> GetByGestor(int idGestor)
        {
            try
            {
                var empresas = await _empresaService.GetByGestorAsync(idGestor);
                return Ok(empresas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
            }
        }

        // POST: api/empresas
        [HttpPost]
        public async Task<ActionResult<EmpresaDto>> Create([FromBody] CreateEmpresaDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var empresa = await _empresaService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = empresa.Id }, empresa);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
            }
        }

        // PUT: api/empresas/5
        [HttpPut("{id}")]
        public async Task<ActionResult<EmpresaDto>> Update(int id, [FromBody] UpdateEmpresaDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var empresa = await _empresaService.UpdateAsync(id, updateDto);
                return Ok(empresa);
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
                return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
            }
        }

        // DELETE: api/empresas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _empresaService.DeleteAsync(id);

                if (!result)
                    return NotFound(new { message = $"No se encontró la empresa con ID {id}" });

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
            }
        }


    }
}
