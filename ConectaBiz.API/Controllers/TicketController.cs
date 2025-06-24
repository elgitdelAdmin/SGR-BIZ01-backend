using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConectaBiz.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly ILogger<TicketController> _logger;

        public TicketController(ITicketService ticketService, ILogger<TicketController> logger)
        {
            _ticketService = ticketService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los tickets
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetAll()
        {
            try
            {
                var tickets = await _ticketService.GetAllAsync();
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los tickets");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene un ticket por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TicketDto>> GetById(int id)
        {
            try
            {
                var ticket = await _ticketService.GetByIdAsync(id);
                if (ticket == null)
                    return NotFound($"No se encontró el ticket con ID: {id}");

                return Ok(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el ticket con ID: {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene un ticket por código
        /// </summary>
        [HttpGet("codigo/{codTicket}")]
        public async Task<ActionResult<TicketDto>> GetByCodTicket(string codTicket)
        {
            try
            {
                var ticket = await _ticketService.GetByCodTicketAsync(codTicket);
                if (ticket == null)
                    return NotFound($"No se encontró el ticket con código: {codTicket}");

                return Ok(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el ticket con código: {CodTicket}", codTicket);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene tickets por empresa
        /// </summary>
        [HttpGet("empresa/{idEmpresa}")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetByEmpresa(int idEmpresa)
        {
            try
            {
                var tickets = await _ticketService.GetByEmpresaAsync(idEmpresa);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tickets por empresa: {IdEmpresa}", idEmpresa);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene tickets por estado
        /// </summary>
        [HttpGet("estado/{idEstado}")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetByEstado(int idEstado)
        {
            try
            {
                var tickets = await _ticketService.GetByEstadoAsync(idEstado);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tickets por estado: {IdEstado}", idEstado);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene tickets por gestor asignado
        /// </summary>
        //[HttpGet("gestor/{idGestor}")]
        //public async Task<ActionResult<IEnumerable<TicketDto>>> GetByGestor(int idGestor)
        //{
        //    try
        //    {
        //        var tickets = await _ticketService.GetByGestorAsync(idGestor);
        //        return Ok(tickets);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al obtener tickets por gestor: {IdGestor}", idGestor);
        //        return StatusCode(500, "Error interno del servidor");
        //    }
        //}

        /// <summary>
        /// Obtiene tickets con filtros opcionales
        /// </summary>
        [HttpGet("filtros")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetWithFilters([FromQuery] int? idEmpresa = null,[FromQuery] int? idEstado = null,[FromQuery] bool? urgente = null)
        {
            try
            {
                var tickets = await _ticketService.GetTicketsWithFiltersAsync(idEmpresa, idEstado, urgente);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tickets con filtros");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene el historial de un ticket
        /// </summary>
        [HttpGet("{id}/historial")]
        public async Task<ActionResult<IEnumerable<TicketHistorialEstadoDto>>> GetHistorial(int id)
        {
            try
            {
                var historial = await _ticketService.GetHistorialByTicketIdAsync(id);
                return Ok(historial);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial del ticket: {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Crea un nuevo ticket
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TicketDto>> Create([FromBody] TicketInsertDto insertDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var ticket = await _ticketService.CreateAsync(insertDto);
                return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear ticket");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el ticket");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Actualiza un ticket existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<TicketDto>> Update(int id, [FromBody] TicketUpdateDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
      
                var ticket = await _ticketService.UpdateAsync(id, updateDto);
                return Ok(ticket);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Ticket no encontrado para actualizar: {Id}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el ticket: {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Elimina un ticket
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _ticketService.DeleteAsync(id);
                if (!deleted)
                    return NotFound($"No se encontró el ticket con ID: {id}");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el ticket: {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
