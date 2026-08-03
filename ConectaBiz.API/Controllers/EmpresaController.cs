using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Common;
using ConectaBiz.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EmpresaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<EmpresaDto>>> GetAll([FromQuery] bool soloActivos = false)
        {
            var empresas = soloActivos
                ? await _empresaService.GetAllActiveAsync()
                : await _empresaService.GetAllAsync();

            return Ok(empresas);
        }

        [HttpGet("byIdSocio/{idSocio}")]
        [ProducesResponseType(typeof(IEnumerable<EmpresaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<EmpresaDto>>> GetByIdSocio(int idSocio)
        {
            var empresas = await _empresaService.GetByIdSocio(idSocio);
            return Ok(empresas);
        }

        [HttpGet("user/{idUser}/rol/{codRol}")]
        [ProducesResponseType(typeof(IEnumerable<EmpresaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<EmpresaDto>>> GetByIdUserIdRolAsync(int idUser, string codRol, [FromQuery] int? idSocio = null)
        {
            var empresas = await _empresaService.GetByIdUserIdRolAsync(idUser, codRol, idSocio);
            return Ok(empresas);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EmpresaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EmpresaDto>> GetById(int id)
        {
            var empresa = await _empresaService.GetByIdAsync(id);

            if (empresa == null)
                throw new KeyNotFoundException($"No se encontró la empresa con ID {id}");

            return Ok(empresa);
        }

        [HttpGet("UsuarioResponsable/tipoDocumento/{idTipoDocumento}/numeroDocumento/{numeroDocumento}")]
        [ProducesResponseType(StatusCodes.Status200OK)] // Devuelve data anonima
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPersonaResponsableByTipoNumDoc(int idTipoDocumento, string numeroDocumento)
        {
            var persona = await _empresaService.GetPersonaResponsableByTipoNumDoc(idTipoDocumento, numeroDocumento);

            if (persona == null)
            {
                throw new KeyNotFoundException($"No se encontró una persona con el número de documento {numeroDocumento}.");
            }

            return Ok(new
            {
                success = true,
                message = "Persona encontrada correctamente.",
                data = persona
            });
        }

        [HttpPost]
        [ProducesResponseType(typeof(EmpresaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Create([FromBody] CreateEmpresaDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var empresa = await _empresaService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = empresa.Id }, empresa);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(EmpresaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EmpresaDto>> Update(int id, [FromBody] UpdateEmpresaDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var empresa = await _empresaService.UpdateAsync(id, updateDto);
            return Ok(empresa);
        }

        [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _empresaService.DeleteAsync(id);

            if (!result)
                throw new KeyNotFoundException($"No se encontró la empresa con ID {id}");

            return NoContent();
        }
    }
}
