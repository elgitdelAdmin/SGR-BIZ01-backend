using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Constants;
using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using ConectaBiz.Domain.Services;
using ConectaBiz.Domain.Strategies.CargaMasiva;
using System.Globalization;
using System.Text.Json;

namespace ConectaBiz.Application.Services
{
    public class CargaMasivaTicketsService : ICargaMasivaTicketsService
    {
        private readonly IExcelService _excelService;
        private readonly ITicketRepository _ticketRepository;
        private readonly IParametrosCatalogo _parametrosCatalogo;
        private readonly IEmpresaService _empresaService;
        private readonly IConsultorService _consultorService;
        private readonly ISubFrenteRepository _subFrenteRepository;
        private readonly CargaMasivaStrategyResolver _strategyResolver;

        public CargaMasivaTicketsService(
            IExcelService excelService,
            ITicketRepository ticketRepository,
            IParametrosCatalogo parametrosCatalogo,
            IEmpresaService empresaService,
            IConsultorService consultorService,
            ISubFrenteRepository subFrenteRepository,
            CargaMasivaStrategyResolver strategyResolver)
        {
            _excelService = excelService;
            _ticketRepository = ticketRepository;
            _parametrosCatalogo = parametrosCatalogo;
            _empresaService = empresaService;
            _consultorService = consultorService;
            _subFrenteRepository = subFrenteRepository;
            _strategyResolver = strategyResolver;
        }

        public async Task<List<Dictionary<string, string>>> ProcesarExcelAsync(Stream stream, string tipo)
        {
            var strategy = _strategyResolver.Resolver(tipo);

            await _parametrosCatalogo.EnsureLoadedAsync();
            var snapshot = _parametrosCatalogo.Current;
            var tipoTicketParams = snapshot.ListaTipoTicket;
            var subTipoTicketParams = snapshot.ListaSubTipoTicket;
            var estadoParams = snapshot.ListaEstados;
            var prioridadParams = snapshot.ListaPrioridades;
            var tipoActividadParams = snapshot.ListaTipoActividad;

            var empresaDto = await _empresaService.GetByNumDocContribuyenteAsync(
                strategy.NumDocContribuyenteEmpresa, AppConstants.Socios.CstiNumDocContribuyente);
            
            if (empresaDto == null)
                throw new Exception($"No se encontró la empresa con RUC '{strategy.NumDocContribuyenteEmpresa}'");

            var ticketsExistentes = await _ticketRepository.GetByNumContribuyenteSocioEmpAsync(AppConstants.Socios.CstiNumDocContribuyente, strategy.NumDocContribuyenteEmpresa);
            var consultores = (await _consultorService.GetAllAsync()).ToList();
            var subFrentes = (await _subFrenteRepository.GetActiveAsync()).ToList();

            var idTipoMesaDeAyuda = tipoTicketParams.FirstOrDefault(t => t.Codigo.Equals(AppConstants.TipoTicket.MesaDeAyuda, StringComparison.OrdinalIgnoreCase))?.Id ?? 0;
            var idTipoActividadAnalisis = tipoActividadParams.FirstOrDefault(t => t.Codigo.Equals(AppConstants.TipoActividad.AnalisisDeRequisitos, StringComparison.OrdinalIgnoreCase))?.Id ?? 0;
            var codigoTipoTicket = tipoTicketParams.FirstOrDefault(t => t.Id == idTipoMesaDeAyuda)?.Codigo ?? "MDA";

            var columnasObligatorias = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "CodTicket", "Titulo", "FechaSolicitud", "EstadoTicket",
                "IdPrioridad", "Descripcion", "UsuarioCreacion", "Asignado"
            };
            var filas = _excelService.LeerFilasComoDict(stream, columnasObligatorias);

            var camposBase = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "CodTicket", "Titulo", "FechaSolicitud", "EstadoTicket",
                "IdPrioridad", "Descripcion", "UsuarioCreacion", "Asignado",
                "GrupoAsignacion", "GrupoAsignación"
            };

            var registros = filas.Select(d =>
            {
                var camposExtra = d.Where(kv => !camposBase.Contains(kv.Key))
                    .ToDictionary(kv => kv.Key, kv => kv.Value);
                string grupoAsignacion = d.TryGetValue("GrupoAsignacion", out var v1) ? v1
                    : d.TryGetValue("GrupoAsignación", out var v2) ? v2 : "";

                return new CargaMasivaGenericoDto
                {
                    CodTicket = d.GetValueOrDefault("CodTicket", ""),
                    Titulo = d.GetValueOrDefault("Titulo", ""),
                    FechaSolicitud = d.GetValueOrDefault("FechaSolicitud", ""),
                    EstadoTicket = d.GetValueOrDefault("EstadoTicket", ""),
                    IdPrioridad = d.GetValueOrDefault("IdPrioridad", ""),
                    Descripcion = d.GetValueOrDefault("Descripcion", ""),
                    UsuarioCreacion = d.GetValueOrDefault("UsuarioCreacion", ""),
                    Asignado = d.GetValueOrDefault("Asignado", ""),
                    GrupoAsignacion = grupoAsignacion,
                    DatosCargaMasiva = JsonSerializer.Serialize(camposExtra)
                };
            }).ToList();

            var ticketsParaCrear = new List<(CargaMasivaGenericoDto Registro, int IdEstado,
                int IdPrioridad, int? IdSubTipo, ConsultorDto? Consultor, DateTime Fecha)>();

            foreach (var reg in registros)
            {
                var codTicketLimpio = strategy.LimpiarCodTicketInterno(reg.CodTicket);

                if (ticketsExistentes.Any(e => e.CodTicketInterno == codTicketLimpio))
                    continue;

                var codigoEstado = strategy.MapearEstadoACodigo(reg.EstadoTicket);
                var idEstado = estadoParams.FirstOrDefault(e =>
                    e.Codigo.Equals(codigoEstado, StringComparison.OrdinalIgnoreCase))?.Id ?? 0;

                var nombrePrioridad = strategy.MapearPrioridadANombre(reg.IdPrioridad);
                var idPrioridad = prioridadParams.FirstOrDefault(p =>
                    p.Nombre.Equals(nombrePrioridad, StringComparison.OrdinalIgnoreCase))?.Id ?? 0;

                var codigoSubTipo = strategy.ObtenerCodigoSubTipoTicket(reg.CodTicket.Trim());
                var idSubTipo = subTipoTicketParams.FirstOrDefault(s =>
                    s.Codigo.Equals(codigoSubTipo, StringComparison.OrdinalIgnoreCase))?.Id;

                var consultor = ConsultorMatchingService.BuscarMejorCoincidencia(
                    reg.Asignado, consultores,
                    c => $"{c.Persona.Nombres} {c.Persona.ApellidoPaterno} {c.Persona.ApellidoMaterno}");

                var fecha = strategy.ParsearFecha(reg.FechaSolicitud, reg.CodTicket);

                ticketsParaCrear.Add((reg, idEstado, idPrioridad, idSubTipo, (ConsultorDto?)consultor, fecha));
            }

            if (!ticketsParaCrear.Any())
            {
                Console.WriteLine("No hay tickets nuevos para insertar.");
                return filas;
            }

            int ultimoId = (await _ticketRepository.GetAllAsync())
                .DefaultIfEmpty().Max(t => t?.Id ?? 0);

            var tickets = ticketsParaCrear.Select((x, index) =>
            {
                int nextId = ultimoId + index + 1;
                string codTicket = $"{codigoTipoTicket}-{DateTime.Now:yyyyMMdd}-{nextId}";

                var asignaciones = new List<TicketConsultorAsignacion>();
                if (x.Consultor != null)
                {
                    var matchingSubFrente = SubFrenteMatchingService.BuscarSubFrentePorGrupo(x.Registro.GrupoAsignacion, subFrentes);
                    asignaciones.Add(new TicketConsultorAsignacion
                    {
                        Id = 0,
                        IdConsultor = x.Consultor.Id,
                        IdTipoActividad = idTipoActividadAnalisis,
                        FechaAsignacion = DateTime.SpecifyKind(x.Fecha, DateTimeKind.Local),
                        FechaDesasignacion = DateTime.SpecifyKind(x.Fecha, DateTimeKind.Local),
                        IdFrente = matchingSubFrente?.IdFrente,
                        IdSubFrente = matchingSubFrente?.Id,
                        Activo = true
                    });
                }

                var frentesSubFrentes = new List<TicketFrenteSubFrente>();
                var fsfMatch = SubFrenteMatchingService.BuscarSubFrentePorGrupo(x.Registro.GrupoAsignacion, subFrentes);
                if (fsfMatch != null)
                {
                    string descCortada = (x.Registro.Descripcion ?? "");
                    if (descCortada.Length > 200) descCortada = descCortada.Substring(0, 200);

                    frentesSubFrentes.Add(new TicketFrenteSubFrente
                    {
                        Id = 0,
                        IdFrente = fsfMatch.IdFrente,
                        IdSubFrente = fsfMatch.Id,
                        Cantidad = 1,
                        Descripcion = descCortada,
                        FechaInicio = DateTime.SpecifyKind(x.Fecha, DateTimeKind.Local),
                        FechaFin = DateTime.SpecifyKind(x.Fecha, DateTimeKind.Local),
                        FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
                        UsuarioCreacion = "CargaMasivaExcel",
                        Activo = true
                    });
                }

                return Ticket.CrearDesdeCargaMasiva(
                    codTicket: codTicket,
                    codTicketInterno: strategy.LimpiarCodTicketInterno(x.Registro.CodTicket),
                    titulo: x.Registro.Titulo,
                    fechaSolicitud: x.Fecha,
                    idTipoTicket: idTipoMesaDeAyuda,
                    idSubTipoTicket: x.IdSubTipo,
                    idEstadoTicket: x.IdEstado,
                    idEmpresa: empresaDto.Id,
                    idUsuarioResponsableCliente: empresaDto.IdUser ?? 0,
                    idPrioridad: x.IdPrioridad,
                    descripcion: x.Registro.Descripcion,
                    idGestorConsultoria: 100,
                    datosCargaMasiva: JsonSerializer.Serialize(x.Registro,
                        new JsonSerializerOptions { WriteIndented = true }),
                    asignaciones: asignaciones,
                    frentesSubFrentes: frentesSubFrentes
                );
            }).ToList();

            await _ticketRepository.CreateRangeAsync(tickets);
            return filas;
        }
    }
}
