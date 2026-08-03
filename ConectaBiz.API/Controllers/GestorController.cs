using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
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
        [ProducesResponseType(typeof(IEnumerable<GestorDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<GestorDto>>> GetAll()
        {
            var gestores = await _gestorService.GetAllAsync();
            return Ok(gestores);
        }

        [HttpGet("byIdSocio/{idSocio}")]
        [ProducesResponseType(typeof(IEnumerable<GestorDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<GestorDto>>> GetByIdSocio(int idSocio)
        {
            var gestores = await _gestorService.GetByIdSocio(idSocio);
            return Ok(gestores);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GestorDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GestorDto>> GetById(int id)
        {
            var gestor = await _gestorService.GetByIdAsync(id);
            if (gestor == null)
            {
                throw new KeyNotFoundException($"No se encontró el gestor con ID {id}");
            }
            return Ok(gestor);
        }

        [HttpGet("byIdRol/{idRol}/byIdSocio/{idSocio}")]
        [ProducesResponseType(typeof(IEnumerable<GestorDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<GestorDto>>> GetByIdRol(int idRol, int idSocio)
        {
            var gestor = await _gestorService.GetByIdRolAsync(idRol, idSocio);
            if (gestor == null)
            {
                throw new KeyNotFoundException($"No se encontró el gestor con ID {idRol}");
            }
            return Ok(gestor);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(GestorDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GestorDto>> Update(int id, [FromBody] UpdateGestorDto updateGestorDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var gestor = await _gestorService.UpdateAsync(id, updateGestorDto);
            return Ok(gestor);
        }

        [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(int id)
        {
            var result = await _gestorService.DeleteAsync(id);
            if (!result)
            {
                throw new KeyNotFoundException($"No se encontró el gestor con ID {id}");
            }
            return NoContent();
        }
    }
}