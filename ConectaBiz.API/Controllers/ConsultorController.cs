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
        public async Task<ActionResult<IEnumerable<ConsultorListDto>>> GetAll()
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los consultores");

                // Si tu servicio devuelve List<ConsultorDto>, usa esto:
                var consultoresDto = await _consultorService.GetAllAsync();
                var consultoresListDto = _mapper.Map<IEnumerable<ConsultorListDto>>(consultoresDto);

                return Ok(consultoresListDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los consultores");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al procesar la solicitud");
            }
        }
        [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.GestorConsultoria},{AppConstants.Roles.GestorCuenta}")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ConsultorDetailDto>> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Obteniendo consultor con ID: {Id}", id);

                var consultor = await _consultorService.GetByIdAsync(id);

                if (consultor == null)
                {
                    _logger.LogWarning("Consultor no encontrado con ID: {Id}", id);
                    return NotFound($"Consultor con ID {id} no encontrado");
                }

                var consultorDetailDto = _mapper.Map<ConsultorDetailDto>(consultor);
                return Ok(consultorDetailDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener consultor con ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al procesar la solicitud");
            }
        }
        [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.GestorConsultoria},{AppConstants.Roles.GestorCuenta}")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ConsultorDto>> Update(int id, [FromBody] UpdateConsultorDto updateConsultorDto)
        {
            try
            {
                _logger.LogInformation("Actualizando consultor con ID: {Id}", id);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido para actualizar consultor con ID: {Id}", id);
                    return BadRequest(ModelState);
                }
                // Mapear el DTO de creación al DTO completo
                var consultorDto = _mapper.Map<ConsultorDto>(updateConsultorDto);

                var updatedConsultor = await _consultorService.UpdateAsync(id, consultorDto);
                _logger.LogInformation("Consultor con ID: {Id} actualizado correctamente", id);

                return Ok(updatedConsultor);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Consultor con ID: {Id} no encontrado para actualizar", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar consultor con ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al procesar la solicitud");
            }
        }

        [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Eliminando consultor con ID: {Id}", id);
                var result = await _consultorService.DeleteAsync(id);

                if (!result)
                {
                    _logger.LogWarning("Consultor con ID: {Id} no encontrado para eliminar", id);
                    return NotFound($"No se encontró el consultor con ID {id}");
                }

                _logger.LogInformation("Consultor con ID: {Id} eliminado correctamente", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar consultor con ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al procesar la solicitud");
            }
        }
    }
}
