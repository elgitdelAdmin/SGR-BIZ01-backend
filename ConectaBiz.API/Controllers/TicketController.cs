using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using Microsoft.AspNetCore.Http.Timeouts;
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
        [HttpGet("user/{idUser}/rol/{codRol}")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetByIdUserIdRolAsync(int idUser, string codRol, [FromQuery] int? idSocio = null)
        {
            try
            {
                var tickets = await _ticketService.GetByIdUserIdRolAsync(idUser, codRol, idSocio);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error al obtener tickets por estado: {IdEstado}", idEstado);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene tickets paginados por usuario y rol
        /// </summary>
        [HttpGet("user/{idUser}/rol/{codRol}/paged")]
        public async Task<ActionResult<PagedResultDto<TicketListItemDto>>> GetPagedByUserRolAsync(
            int idUser, string codRol,
            [FromQuery] int? idSocio = null,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? estadoIds = null,
            [FromQuery] string? globalFilter = null,
            [FromQuery] string? sortField = null,
            [FromQuery] string? sortOrder = null,
            // Nuevos filtros
            [FromQuery] string? codTicket = null,
            [FromQuery] string? codTicketInterno = null,
            [FromQuery] string? titulo = null,
            [FromQuery] string? empresa = null,
            [FromQuery] string? gestor = null,
            [FromQuery] string? prioridad = null,
            [FromQuery] string? estado = null,
            [FromQuery] string? nombreConsultor = null
            )
        {
            try
            {
                List<int>? estadoIdsList = null;
                if (!string.IsNullOrWhiteSpace(estadoIds))
                {
                    estadoIdsList = estadoIds.Split(',')
                        .Where(s => int.TryParse(s, out _))
                        .Select(int.Parse)
                        .ToList();
                }

                Console.WriteLine($"[DEBUG-CONTROLLER] nombreConsultor received: '{nombreConsultor}'");
                Console.WriteLine($"[DEBUG-CONTROLLER] gestor received: '{gestor}'");
                Console.WriteLine($"[DEBUG-CONTROLLER] empresa received: '{empresa}'");
                Console.WriteLine($"[DEBUG-CONTROLLER] estado received: '{estado}'");
                var result = await _ticketService.GetPagedByUserRolAsync(
                    idUser, codRol, idSocio, page, pageSize,
                    estadoIdsList, globalFilter, sortField, sortOrder,
                    codTicket, codTicketInterno, titulo, empresa, gestor, prioridad, estado,
                    nombreConsultor);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tickets paginados");
                return StatusCode(500, "Error interno del servidor");
            }
        }
        
        /// <summary>
        /// Obtiene tickets por usuario
        /// </summary>
        //[HttpGet("usuario/{idUser}")]
        //public async Task<ActionResult<IEnumerable<TicketDto>>> GetByIdUserAsync(int idUser)
        //{
        //    try
        //    {
        //        var tickets = await _ticketService.GetByIdUserAsync(idUser);
        //        return Ok(tickets);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al obtener tickets por idUsuario: {idUser}", idUser);
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
        [RequestTimeout(120000)] // 2 minutos
        public async Task<ActionResult<TicketDto>> Create([FromForm] TicketInsertDto insertDto)
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
                return StatusCode(500, ex.Message);
            }
        }
        //[HttpPost]
        //public async Task<ActionResult<TicketDto>> CrearTicket([FromBody] TicketInsertDto insertDto)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //            return BadRequest(ModelState);

        //        var ticket = await _ticketService.CreateAsync(insertDto);
        //        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        _logger.LogWarning(ex, "Error de validación al crear ticket");
        //        return BadRequest(ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al crear el ticket");
        //        return StatusCode(500, "Error interno del servidor");
        //    }
        //}
        //// Subir ZIP
        //[HttpPost("subir-archivo/{ticketId}")]
        //public async Task<ActionResult<TicketZipFileDto>> SubirArchivo(int ticketId, IFormFile zipFile)
        //{
        //    try
        //    {
        //        var result = await _ticketService.UploadZipFileAsync(ticketId, zipFile);
        //        return Ok(result);
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        _logger.LogWarning(ex, "Error al subir archivo");
        //        return BadRequest(ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error interno al subir archivo");
        //        return StatusCode(500, "Error interno del servidor");
        //    }
        //}
        /// <summary>
        /// Actualiza un ticket existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<TicketDto>> Update(int id, [FromForm] TicketUpdateDto updateDto)
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
                return StatusCode(500, ex.Message);
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

        [HttpGet("{idTicket}/desgarcarArchivo/{orden}")]
        public async Task<IActionResult> DescargarArchivoTicket(int idTicket, int orden)
        {
            try
            {
                var fileResult = await _ticketService.DescargarArchivoAsync(idTicket, orden);
                return fileResult;
            }
            catch (FileNotFoundException)
            {
                return NotFound("Archivo no encontrado.");
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        /// <summary>
        /// Migra un ticket por su código de requerimiento SGR
        /// </summary>
        [HttpPost("migrarsgr/{codTicketInterno}")]
        public async Task<ActionResult> MigrarTicket([FromServices] ISGRCSTIService sgrcstiService, string codTicketInterno)
        {
            try
            {
                var resultados = await sgrcstiService.MigracionRequerimientoPorCodAsync(codTicketInterno);
                return Ok(new { 
                    mensaje = "Proceso finalizado.", 
                    migrados = resultados 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al migrar el ticket {CodTicketInterno}", codTicketInterno);
                return StatusCode(500, new { mensaje = "Error interno durante la migración.", detalle = ex.Message });
            }
        }

    }
}
