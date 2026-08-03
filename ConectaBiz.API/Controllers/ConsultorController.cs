using AutoMapper;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Application.Services;
using ConectaBiz.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConectaBiz.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultorController : ControllerBase
    {
        private readonly IConsultorService _consultorService;
        private readonly ILogger<ConsultorController> _logger;
        private readonly IMapper _mapper;

        public ConsultorController(
            IConsultorService consultorService,
            ILogger<ConsultorController> logger,
            IMapper mapper
            )
        {
            _consultorService = consultorService;
            _logger = logger;
            _mapper = mapper;
        }

        [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.GestorConsultoria},{AppConstants.Roles.GestorCuenta}")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ConsultorListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ConsultorListDto>>> GetAll()
        {
            _logger.LogInformation("Obteniendo todos los consultores");
            var consultoresDto = await _consultorService.GetAllAsync();
            var consultoresListDto = _mapper.Map<IEnumerable<ConsultorListDto>>(consultoresDto);
            return Ok(consultoresListDto);
        }

        [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.GestorConsultoria},{AppConstants.Roles.GestorCuenta}")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ConsultorDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ConsultorDetailDto>> GetById(int id)
        {
            _logger.LogInformation("Obteniendo consultor con ID: {Id}", id);
            var consultor = await _consultorService.GetByIdAsync(id);

            if (consultor == null)
            {
                _logger.LogWarning("Consultor no encontrado con ID: {Id}", id);
                throw new KeyNotFoundException($"Consultor con ID {id} no encontrado");
            }

            var consultorDetailDto = _mapper.Map<ConsultorDetailDto>(consultor);
            return Ok(consultorDetailDto);
        }

        [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.GestorConsultoria},{AppConstants.Roles.GestorCuenta}")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ConsultorDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ConsultorDto>> Update(int id, [FromBody] UpdateConsultorDto updateConsultorDto)
        {
            _logger.LogInformation("Actualizando consultor con ID: {Id}", id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido para actualizar consultor con ID: {Id}", id);
                return BadRequest(ModelState);
            }

            var consultorDto = _mapper.Map<ConsultorDto>(updateConsultorDto);
            var updatedConsultor = await _consultorService.UpdateAsync(id, consultorDto);
            _logger.LogInformation("Consultor con ID: {Id} actualizado correctamente", id);

            return Ok(updatedConsultor);
        }

        [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(int id)
        {
            _logger.LogInformation("Eliminando consultor con ID: {Id}", id);
            var result = await _consultorService.DeleteAsync(id);

            if (!result)
            {
                _logger.LogWarning("Consultor con ID: {Id} no encontrado para eliminar", id);
                throw new KeyNotFoundException($"No se encontró el consultor con ID {id}");
            }

            _logger.LogInformation("Consultor con ID: {Id} eliminado correctamente", id);
            return NoContent();
        }
    }
}
