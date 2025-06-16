using AutoMapper;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Application.Services;
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


        //[HttpGet]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //public async Task<ActionResult<IEnumerable<ConsultorListDto>>> GetAll()
        //{
        //    try
        //    {
        //        _logger.LogInformation("Obteniendo todos los consultores");

        //        var consultores = await _consultorService.GetAllAsync();
        //        var consultoresListDto = _mapper.Map<IEnumerable<ConsultorListDto>>(consultores);

        //        return Ok(consultoresListDto);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al obtener todos los consultores");
        //        return StatusCode(StatusCodes.Status500InternalServerError, "Error al procesar la solicitud");
        //    }
        //}

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ConsultorListDto>>> GetAll()
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los consultores");

                // Si tu servicio devuelve List<ConsultorDto>, usa esto:
                var consultoresDto = await _consultorService.GetAllAsync();
                var consultoresListDto = _mapper.Map<IEnumerable<ConsultorListDto>>(consultoresDto);

                // O si cambias tu servicio para devolver entidades Consultor, usa esto:
                // var consultores = await _consultorService.GetAllEntitiesAsync();
                // var consultoresListDto = _mapper.Map<IEnumerable<ConsultorListDto>>(consultores);

                return Ok(consultoresListDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los consultores");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al procesar la solicitud");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Crea un nuevo consultor
        /// </summary>
        /// <param name="consultorDto">Datos del consultor</param>
        /// <returns>Datos del consultor creado</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ConsultorDetailDto>> Create([FromBody] CreateConsultorDto createConsultorDto)
        {
            try
            {
                _logger.LogInformation("Creando nuevo consultor");

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido para crear consultor");
                    return BadRequest(ModelState);
                }

                // Mapear el DTO de creación al DTO completo
                var consultorDto = _mapper.Map<ConsultorDto>(createConsultorDto);

                var createdConsultor = await _consultorService.CreateAsync(consultorDto);
                _logger.LogInformation("Consultor creado con ID: {Id}", createdConsultor.Id);

                // Obtener el consultor completo para la respuesta
                var consultorCompleto = await _consultorService.GetByIdAsync(createdConsultor.Id);
                var consultorDetailDto = _mapper.Map<ConsultorDetailDto>(consultorCompleto);

                return CreatedAtAction(nameof(GetById), new { id = createdConsultor.Id }, consultorDetailDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear consultor");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear consultor");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al procesar la solicitud");
            }
        }
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
