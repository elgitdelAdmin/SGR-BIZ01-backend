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

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetAll()
        {
            var tickets = await _ticketService.GetAllAsync();
            return Ok(tickets);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TicketDto>> GetById(int id)
        {
            var ticket = await _ticketService.GetByIdAsync(id);
            if (ticket == null)
                throw new KeyNotFoundException($"No se encontró el ticket con ID: {id}");

            return Ok(ticket);
        }

        [HttpGet("codigo/{codTicket}")]
        [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TicketDto>> GetByCodTicket(string codTicket)
        {
            var ticket = await _ticketService.GetByCodTicketAsync(codTicket);
            if (ticket == null)
                throw new KeyNotFoundException($"No se encontró el ticket con código: {codTicket}");

            return Ok(ticket);
        }

        [HttpGet("empresa/{idEmpresa}")]
        [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetByEmpresa(int idEmpresa)
        {
            var tickets = await _ticketService.GetByEmpresaAsync(idEmpresa);
            return Ok(tickets);
        }

        [HttpGet("estado/{idEstado}")]
        [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetByEstado(int idEstado)
        {
            var tickets = await _ticketService.GetByEstadoAsync(idEstado);
            return Ok(tickets);
        }

        [HttpGet("user/{idUser}/rol/{codRol}")]
        [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetByIdUserIdRolAsync(int idUser, string codRol, [FromQuery] int? idSocio = null)
        {
            var tickets = await _ticketService.GetByIdUserIdRolAsync(idUser, codRol, idSocio);
            return Ok(tickets);
        }

        [HttpGet("user/{idUser}/rol/{codRol}/paged")]
        [ProducesResponseType(typeof(PagedResultDto<TicketListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResultDto<TicketListItemDto>>> GetPagedByUserRolAsync(
            int idUser, string codRol,
            [FromQuery] int? idSocio = null,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? estadoIds = null,
            [FromQuery] string? globalFilter = null,
            [FromQuery] string? sortField = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? codTicket = null,
            [FromQuery] string? codTicketInterno = null,
            [FromQuery] string? titulo = null,
            [FromQuery] string? empresa = null,
            [FromQuery] string? gestor = null,
            [FromQuery] string? prioridad = null,
            [FromQuery] string? estado = null,
            [FromQuery] string? nombreConsultor = null,
            [FromQuery] string? tipoSubtipo = null
            )
        {
            List<int>? estadoIdsList = null;
            if (!string.IsNullOrWhiteSpace(estadoIds))
            {
                estadoIdsList = estadoIds.Split(',')
                    .Where(s => int.TryParse(s, out _))
                    .Select(int.Parse)
                    .ToList();
            }

            var result = await _ticketService.GetPagedByUserRolAsync(
                idUser, codRol, idSocio, page, pageSize,
                estadoIdsList, globalFilter, sortField, sortOrder,
                codTicket, codTicketInterno, titulo, empresa, gestor, prioridad, estado,
                nombreConsultor, tipoSubtipo);

            return Ok(result);
        }
        
        //[HttpGet("filtros")]
        //[ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult<IEnumerable<TicketDto>>> GetWithFilters([FromQuery] int? idEmpresa = null,[FromQuery] int? idEstado = null,[FromQuery] bool? urgente = null)
        //{
        //    var tickets = await _ticketService.GetTicketsWithFiltersAsync(idEmpresa, idEstado, urgente);
        //    return Ok(tickets);
        //}

        [HttpGet("{id}/historial")]
        [ProducesResponseType(typeof(IEnumerable<TicketHistorialEstadoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<TicketHistorialEstadoDto>>> GetHistorial(int id)
        {
            var historial = await _ticketService.GetHistorialByTicketIdAsync(id);
            return Ok(historial);
        }

        [HttpPost]
        [RequestTimeout(120000)]
        [ProducesResponseType(typeof(TicketDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TicketDto>> Create([FromBody] TicketInsertDto insertDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ticket = await _ticketService.CreateAsync(insertDto);
            return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TicketDto>> Update(int id, [FromBody] TicketUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
    
            var ticket = await _ticketService.UpdateAsync(id, updateDto);
            return Ok(ticket);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(int id)
        {
            var deleted = await _ticketService.DeleteAsync(id);
            if (!deleted)
                throw new KeyNotFoundException($"No se encontró el ticket con ID: {id}");

            return NoContent();
        }

        [HttpGet("{idTicket}/desgarcarArchivo/{orden}")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DescargarArchivoTicket(int idTicket, int orden)
        {
            var fileResult = await _ticketService.DescargarArchivoAsync(idTicket, orden);
            return File(fileResult.Content, fileResult.ContentType, fileResult.FileName);
        }

        [HttpPost("migrarsgr/{codTicketInterno}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> MigrarTicket([FromServices] ISGRCSTIService sgrcstiService, string codTicketInterno)
        {
            var resultados = await sgrcstiService.MigracionRequerimientoPorCodAsync(codTicketInterno);
            return Ok(new { 
                mensaje = "Proceso finalizado.", 
                migrados = resultados 
            });
        }
    }
}
