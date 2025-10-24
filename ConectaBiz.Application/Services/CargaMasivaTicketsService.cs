using AutoMapper;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Constants;
using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Globalization;
using System.Text.Json;
using static NPOI.HSSF.Util.HSSFColor;

public class CargaMasivaTicketsService : ICargaMasivaTicketsService
{
    private readonly IEmpresaRepository _empresaRepository;
    private readonly ICargaMasivaTicketsRepository _cargaMasivaTicketsRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IParametroRepository _parametroRepository;
    private readonly IEmpresaService _empresaService;
    private readonly IConsultorService _consultorService;
    private readonly ITicketConsultorAsignacionRepository _consultorAsignacionRepository;
    private readonly IMapper _mapper;

    // 🔹 Variables para cachear los datos que cargamos en ProcesarExcelAsync
    private IEnumerable<Parametro> _listaTipoTicket;
    private IEnumerable<Parametro> _listaEstados;
    private IEnumerable<Parametro> _listaPrioridades;
    private EmpresaDto _empresaDto;
    private IEnumerable<ConsultorDto> _listaConsultores;
    private IEnumerable<Parametro> _listaParametros;
    private IEnumerable<Parametro> _listaTipoActividad;
    private IEnumerable<Ticket> _listaTicketsExistentes;
    private string _tipoCarga;
    private int? _tipoTicket;

    public CargaMasivaTicketsService(
        IEmpresaRepository empresaRepository,
        ITicketConsultorAsignacionRepository consultorAsignacionRepository,
        ICargaMasivaTicketsRepository cargaMasivaTicketsRepository,
        ITicketRepository ticketRepository,
        IParametroRepository parametroRepository,
        IEmpresaService empresaService,
        IConsultorService consultorService,
        IMapper mapper)
    {
        _empresaRepository = empresaRepository;
        _consultorAsignacionRepository = consultorAsignacionRepository;
        _cargaMasivaTicketsRepository = cargaMasivaTicketsRepository;
        _ticketRepository = ticketRepository;
        _parametroRepository = parametroRepository;
        _empresaService = empresaService;
        _consultorService = consultorService;
        _mapper = mapper;
    }

    public async Task<List<Dictionary<string, string>>> ProcesarExcelAsync(Stream stream, string tipo)
    {
        try
        {
            // 🔹 Determinar empresa según tipo
            string numDocContribuyenteEmpresa = tipo switch
            {
                AppConstants.TipoCargaMasiva.IncidentesAlicorp => AppConstants.Empresas.AlicorpNumDocContribuyente,
                AppConstants.TipoCargaMasiva.RequerimientosAlicorp => AppConstants.Empresas.AlicorpNumDocContribuyente,
                AppConstants.TipoCargaMasiva.TicketsExcelia => AppConstants.Empresas.ExceliaNumDocContribuyente,
                AppConstants.TipoCargaMasiva.TicketsRansa => AppConstants.Empresas.RansaNumDocContribuyente,
                _ => throw new Exception("Tipo de carga masiva no soportado")
            };

            _tipoCarga = tipo;

            // 🔹 Cargar todos los datos necesarios
            await InicializarDatosAsync(numDocContribuyenteEmpresa);

            // 🔹 Determinar empresa según tipo
            _tipoTicket = tipo switch
            {
                AppConstants.TipoCargaMasiva.IncidentesAlicorp => ObtenerTipoTicketPorCodigo(AppConstants.TipoTicket.Incidencia) ,
                AppConstants.TipoCargaMasiva.RequerimientosAlicorp => ObtenerTipoTicketPorCodigo(AppConstants.TipoTicket.Requerimiento),
                AppConstants.TipoCargaMasiva.TicketsExcelia => null,
                AppConstants.TipoCargaMasiva.TicketsRansa => null,
                _ => throw new Exception("Tipo de carga masiva no soportado")
            };


            // Abrir Excel
            XSSFWorkbook workbook = new XSSFWorkbook(stream);
            var sheet = workbook.GetSheetAt(0);
            if (sheet == null) return new List<Dictionary<string, string>>();
            return tipo switch
            {
                
                AppConstants.TipoCargaMasiva.IncidentesAlicorp => await InsertarGenericoAsync(sheet),
                AppConstants.TipoCargaMasiva.RequerimientosAlicorp => await InsertarGenericoAsync(sheet),
                AppConstants.TipoCargaMasiva.TicketsExcelia => await InsertarGenericoAsync(sheet),
                AppConstants.TipoCargaMasiva.TicketsRansa => await InsertarGenericoAsync(sheet),
                _ => throw new Exception("Tipo de carga masiva no soportado")
            };
        }
        catch (Exception ex)
        {
            throw;
        }
    }


    // 🔹 Cargar todos los datos necesarios
    private async Task InicializarDatosAsync(string numDocContribuyenteEmpresa)
    {
        _listaParametros = await _parametroRepository.GetAllAsync();
        _listaTipoTicket = _listaParametros.Where(p => p.TipoParametro == AppConstants.TiposParametros.TipoTicket).ToList();
        _listaEstados = _listaParametros.Where(p => p.TipoParametro == AppConstants.TiposParametros.EstadoTicket).ToList();
        _listaPrioridades = _listaParametros.Where(p => p.TipoParametro == AppConstants.TiposParametros.Prioridad).ToList();
        _listaTipoActividad = _listaParametros.Where(p => p.TipoParametro == AppConstants.TiposParametros.TipoActividad).ToList();
        _empresaDto = await _empresaService.GetByNumDocContribuyenteAsync(numDocContribuyenteEmpresa,AppConstants.Socios.CstiNumDocContribuyente);
        _listaTicketsExistentes = await _ticketRepository.GetByNumContribuyenteSocioEmpAsync(AppConstants.Socios.CstiNumDocContribuyente, numDocContribuyenteEmpresa);
        _listaConsultores = await _consultorService.GetByNumDocContribuyenteSocioAsync(AppConstants.Socios.CstiNumDocContribuyente);
    }

    // Obtener consultor
    private ConsultorDto? BuscarConsultorPorNombreCompleto(string nombreCompleto)
    {
        if (string.IsNullOrWhiteSpace(nombreCompleto) || _listaConsultores == null)
            return null;

        // 🔹 Normalizar el nombre (quitar dobles espacios, minúsculas)
        var partes = nombreCompleto
            .Split(" ", StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim().ToLower())
            .ToList();

        foreach (var consultor in _listaConsultores)
        {
            var persona = consultor.Persona;

            // Construimos el nombre completo de la persona en minúsculas
            var nombrePersona = $"{persona.Nombres} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".ToLower();

            // Verificamos si todas las partes del input están contenidas en el nombrePersona
            bool coincide = partes.All(p => nombrePersona.Contains(p));

            if (coincide)
            {
                return consultor;
            }
        }
        return null; // Si no encuentra coincidencia
    }

    // 🔹 Mapeo Tipo Ticket
    private int ObtenerTipoActividadPorCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return 0;
        var tipoActividad = _listaTipoActividad.FirstOrDefault(t => t.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase));
        return tipoActividad != null ? tipoActividad.Id : 0;
    }

    // 🔹 Mapeo Tipo Ticket
    private int LogicaObtenerTipoTicketPorCodigo(string codigo)
    {
        int idTipoTicket = 0;

        if (_tipoCarga == AppConstants.TipoCargaMasiva.TicketsExcelia)
        {
            idTipoTicket = codigo switch
            {
                AppConstants.TipoCargaMasiva.TipoCargaMasivaTicketExcelia.Requerimientos => ObtenerTipoTicketPorCodigo(AppConstants.TipoTicket.Requerimiento),
                AppConstants.TipoCargaMasiva.TipoCargaMasivaTicketExcelia.Incidentes => ObtenerTipoTicketPorCodigo(AppConstants.TipoTicket.Incidencia),
                _ => throw new Exception("LogicaObtenerTipoTicketPorCodigo() no soportado")
            };
        }
        if (_tipoCarga == AppConstants.TipoCargaMasiva.TicketsRansa)
        {
            idTipoTicket = codigo switch
            {
                AppConstants.TipoCargaMasiva.TipoCargaMasivaTicketRansa.Requerimientos => ObtenerTipoTicketPorCodigo(AppConstants.TipoTicket.Requerimiento),
                AppConstants.TipoCargaMasiva.TipoCargaMasivaTicketRansa.Incidentes => ObtenerTipoTicketPorCodigo(AppConstants.TipoTicket.Incidencia),
                _ => throw new Exception("LogicaObtenerTipoTicketPorCodigo() no soportado")
            };
        }
        return idTipoTicket;
    }

    private int ObtenerTipoTicketPorCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return 0;
        var tipoTicket = _listaTipoTicket.FirstOrDefault(t =>t.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase));
        return tipoTicket != null ? tipoTicket.Id : 0;
    }

    // 🔹 Mapeo Estado
    private int MapearEstado(string estadoRecibido)
    {
        if (string.IsNullOrWhiteSpace(estadoRecibido)) return 0;
        var mapeo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Asignado", "APROBADO" },
            { "Cancelado", "CANCELADO" },
            { "Cerrado", "CERRADO" },
            { "Pendiente", "PENDIENTE_APROBACION" },
            { "Resuelto", "CERRADO" }
        };
        if (mapeo.TryGetValue(estadoRecibido.Trim(), out var codigoInterno))
        {
            var estado = _listaEstados.FirstOrDefault(e =>
                e.Codigo.Equals(codigoInterno, StringComparison.OrdinalIgnoreCase));

            if (estado != null)
                return Convert.ToInt32(estado.Id);
        }
        return 0;
    }

    // 🔹 Mapeo Prioridad
    private int MapearPrioridad(string prioridadExcel)
    {
        if (string.IsNullOrWhiteSpace(prioridadExcel)) return 0;

        var partes = prioridadExcel.Split('-', 2);
        string nombrePrioridad = partes.Length == 2 ? partes[1].Trim() : prioridadExcel.Trim();

        var mapeoNombres = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Medio", "Media" },
            { "Baja", "Baja" },
            { "Alta", "Alta" },
            { "Crítica", "Crítica" }
        };

        if (mapeoNombres.TryGetValue(nombrePrioridad, out var nombreNormalizado))
            nombrePrioridad = nombreNormalizado;

        var prioridad = _listaPrioridades.FirstOrDefault(p =>
            p.Nombre.Equals(nombrePrioridad, StringComparison.OrdinalIgnoreCase));

        return prioridad != null ? Convert.ToInt32(prioridad.Id) : 0;
    }


    private async Task<List<Dictionary<string, string>>> InsertarGenericoAsync(ISheet sheet)
    {
        var datos = new List<Dictionary<string, string>>();

        var headerRow = sheet.GetRow(0);
        int colCount = headerRow.LastCellNum;

        for (int rowIdx = 1; rowIdx <= sheet.LastRowNum; rowIdx++)
        {
            var row = sheet.GetRow(rowIdx);
            if (row == null) continue;

            // ✅ Omitir filas completamente vacías
            bool filaVacia = true;
            for (int colIdx = 0; colIdx < colCount; colIdx++)
            {
                var celda = row.GetCell(colIdx);
                if (!string.IsNullOrWhiteSpace(celda?.ToString()))
                {
                    filaVacia = false;
                    break; // Si encontramos una celda con valor, no es fila vacía
                }
            }
            if (filaVacia) continue;

            var fila = new Dictionary<string, string>();
            for (int colIdx = 0; colIdx < colCount; colIdx++)
            {
                string header = headerRow.GetCell(colIdx)?.ToString() ?? $"Column{colIdx}";
                string value = row.GetCell(colIdx)?.ToString() ?? "";
                fila[header] = value;
            }

            datos.Add(fila);
        }


        // ✅ Lista de campos "principales" que no deben ir en el JSON
        var camposBase = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CodTicket",
            "Titulo",
            "FechaSolicitud",
            "EstadoTicket",
            "IdPrioridad",
            "Descripcion",
            "UsuarioCreacion",
            "Asignado"
        };

        // ✅ Convertir filas del Excel en objetos DTO
        var incidentesDto = datos.Select(d =>
        {
            // Extraer solo los campos adicionales para el JSON
            var camposExtra = d
                .Where(kv => !camposBase.Contains(kv.Key))
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            string jsonExtras = System.Text.Json.JsonSerializer.Serialize(camposExtra, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            return new CargaMasivaGenericoDto
            {
                CodTicket = d.ContainsKey("CodTicket") ? d["CodTicket"] : "",
                Titulo = d.ContainsKey("Titulo") ? d["Titulo"] : "",
                FechaSolicitud = d.ContainsKey("FechaSolicitud") ? d["FechaSolicitud"] : "",
                EstadoTicket = d.ContainsKey("EstadoTicket") ? d["EstadoTicket"] : "",
                IdPrioridad = d.ContainsKey("IdPrioridad") ? d["IdPrioridad"] : "",
                Descripcion = d.ContainsKey("Descripcion") ? d["Descripcion"] : "",
                UsuarioCreacion = d.ContainsKey("UsuarioCreacion") ? d["UsuarioCreacion"] : "",
                Asignado = d.ContainsKey("Asignado") ? d["Asignado"] : "",
                DatosCargaMasiva = jsonExtras
            };
        }).ToList();

        var ticketsInsertDto = incidentesDto.Select(i =>
        {
            try
            {
                // 🔹 Revisar campos importantes antes de usar .Value
                if (_empresaDto == null)
                    throw new Exception($"_empresaDto es null para ticket {i.CodTicket}");

                // 🔹 Buscar consultor
                var consultor = BuscarConsultorPorNombreCompleto(i.Asignado);

                // 🔹 Parsear fecha
                DateTime fechaAsignacion;
                try
                {
                    if (!string.IsNullOrWhiteSpace(i.FechaSolicitud))
                    {
                        bool parseoExitoso = DateTime.TryParseExact(
                            i.FechaSolicitud.Trim(),
                            new[] { "dd-MMM-yyyy", "dd/MM/yyyy HH:mm:ss" }, // múltiples formatos
                            new CultureInfo("es-PE"),
                            DateTimeStyles.None,
                            out fechaAsignacion
                        );

                        if (!parseoExitoso)
                            fechaAsignacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                        else
                            fechaAsignacion = DateTime.SpecifyKind(fechaAsignacion, DateTimeKind.Local);
                    }
                    else
                    {
                        fechaAsignacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error parseando fecha Creado '{i.FechaSolicitud}' para ticket {i.CodTicket}: {ex.Message}");
                }

                // 🔹 Crear asignación consultor
                TicketConsultorAsignacionInsertDto? ticketConsultorInsertDto = null;
                try
                {
                    if (consultor != null)
                    {
                        ticketConsultorInsertDto = new TicketConsultorAsignacionInsertDto
                        {
                            IdConsultor = consultor.Id,
                            IdTipoActividad = ObtenerTipoActividadPorCodigo(AppConstants.TipoActividad.AnalisisDeRequisitos),
                            FechaAsignacion = fechaAsignacion,
                            FechaDesasignacion = fechaAsignacion
                        };
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error creando TicketConsultorAsignacionInsertDto para ticket {i.CodTicket}: {ex.Message}");
                }

                // 🔹 Construir TicketInsertDto
                return new TicketInsertMasivoDto
                {
                    CodTicketInterno = i.CodTicket,
                    Titulo = i.Titulo,
                    FechaSolicitud = fechaAsignacion,
                    IdTipoTicket = _tipoTicket ??= LogicaObtenerTipoTicketPorCodigo(i.CodTicket.Trim().Substring(0, 4)),
                    IdEstadoTicket = MapearEstado(i.EstadoTicket),
                    IdEmpresa = _empresaDto.Id,
                    IdUsuarioResponsableCliente = _empresaDto.IdUser,
                    IdPrioridad = MapearPrioridad(i.IdPrioridad),
                    Descripcion = i.Descripcion,
                    UsuarioCreacion = i.UsuarioCreacion,
                    IdGestor = (int)_empresaDto.IdGestor,
                    EsCargaMasiva = true,
                    DatosCargaMasiva = System.Text.Json.JsonSerializer.Serialize(i, new JsonSerializerOptions { WriteIndented = true }),
                    ConsultorAsignaciones = ticketConsultorInsertDto != null
                        ? new List<TicketConsultorAsignacionInsertDto> { ticketConsultorInsertDto }
                        : new List<TicketConsultorAsignacionInsertDto>()
                };
            }
            catch (Exception exInner)
            {
                Console.WriteLine($"❌ Error procesando ticket '{i.CodTicket}': {exInner.Message}");
                throw;
            }
        }).ToList();

        // ✅ Filtrar duplicados antes de insertar
        var nuevosTickets = ticketsInsertDto
            .Where(t => !_listaTicketsExistentes.Any(e => e.CodTicketInterno == t.CodTicketInterno))
            .ToList();

        if (!nuevosTickets.Any())
        {
            Console.WriteLine("⚠️ No hay tickets nuevos para insertar, todos ya existen en la base de datos.");
            return datos;
        }

        // ✅ Insertar solo los nuevos
        await CreateMasivoAsync(nuevosTickets);
        return datos;
    }

    // 🔹 Incidentes usando las lógicas ya refactorizadas
    private async Task<List<Dictionary<string, string>>> InsertarIncidentesAlicorpAsync(ISheet sheet)
    {
        var datos = new List<Dictionary<string, string>>();

        var headerRow = sheet.GetRow(0);
        int colCount = headerRow.LastCellNum;

        for (int rowIdx = 1; rowIdx <= sheet.LastRowNum; rowIdx++)
        {
            var row = sheet.GetRow(rowIdx);
            if (row == null) continue;

            var fila = new Dictionary<string, string>();
            for (int colIdx = 0; colIdx < colCount; colIdx++)
            {
                string header = headerRow.GetCell(colIdx)?.ToString() ?? $"Column{colIdx}";
                string value = row.GetCell(colIdx)?.ToString() ?? "";
                fila[header] = value;
            }
            datos.Add(fila);
        }

        var incidentesAlicorpDto = datos.Select(d => new CargaMasivaIncidentesAlicorpDto 
        { 
            Numero = d.ContainsKey("Número") ? d["Número"] : "", 
            Solicitante = d.ContainsKey("Solicitante") ? d["Solicitante"] : "", 
            UsuarioFinalAfectado = d.ContainsKey("Usuario Final Afectado") ? d["Usuario Final Afectado"] : "", 
            Canal = d.ContainsKey("Canal") ? d["Canal"] : "", 
            Estado = d.ContainsKey("Estado") ? d["Estado"] : "", 
            MotivosParaPonerEnEspera = d.ContainsKey("Motivos para poner en espera") ? d["Motivos para poner en espera"] : "", 
            BreveDescripcion = d.ContainsKey("Breve descripción") ? d["Breve descripción"] : "", 
            Descripcion = d.ContainsKey("Descripción") ? d["Descripción"] : "", 
            Prioridad = d.ContainsKey("Prioridad") ? d["Prioridad"] : "", 
            Urgencia = d.ContainsKey("Urgencia") ? d["Urgencia"] : "", 
            Impacto = d.ContainsKey("Impacto") ? d["Impacto"] : "", 
            SedeDelIncidente = d.ContainsKey("Sede del Incidente") ? d["Sede del Incidente"] : "", 
            Sociedad = d.ContainsKey("Sociedad") ? d["Sociedad"] : "", 
            ElementoDeConfiguracion = d.ContainsKey("Elemento de configuración") ? d["Elemento de configuración"] : "", 
            Servicio = d.ContainsKey("Servicio") ? d["Servicio"] : "", 
            Categoria1 = d.ContainsKey("Categoría 1") ? d["Categoría 1"] : "", 
            Categoria2 = d.ContainsKey("Categoría 2") ? d["Categoría 2"] : "", 
            Categoria3 = d.ContainsKey("Categoría 3") ? d["Categoría 3"] : "", 
            GrupoDeAsignacion = d.ContainsKey("Grupo de asignación") ? d["Grupo de asignación"] : "", 
            AsignadoA = d.ContainsKey("Asignado a") ? d["Asignado a"] : "", 
            TicketExterno = d.ContainsKey("Ticket externo") ? d["Ticket externo"] : "", 
            Creados = d.ContainsKey("Creados") ? d["Creados"] : "", 
            CreadosPor = d.ContainsKey("Creados por") ? d["Creados por"] : "", 
            Actualizados = d.ContainsKey("Actualizados") ? d["Actualizados"] : "", 
            ActualizadoPor = d.ContainsKey("Actualizado por") ? d["Actualizado por"] : "", 
            NivelFuncional = d.ContainsKey("Nivel Funcional") ? d["Nivel Funcional"] : "" }).ToList();

            var ticketsInsertDto = incidentesAlicorpDto.Select(i =>
            {
                try
                {
                    // 🔹 Revisar campos importantes antes de usar .Value
                    if (_empresaDto == null)
                        throw new Exception($"_empresaDto es null para ticket {i.Numero}");



                    // 🔹 Buscar consultor
                    var consultor = BuscarConsultorPorNombreCompleto(i.AsignadoA);

                    // 🔹 Parsear fecha
                    DateTime fechaAsignacion;
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(i.Creados))
                        {
                            bool parseoExitoso = DateTime.TryParseExact(
                                i.Creados.Trim(),
                                new[] { "dd-MMM-yyyy", "dd/MM/yyyy HH:mm:ss" }, // múltiples formatos
                                new CultureInfo("es-PE"),
                                DateTimeStyles.None,
                                out fechaAsignacion
                            );

                            if (!parseoExitoso)
                                fechaAsignacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                            else
                                fechaAsignacion = DateTime.SpecifyKind(fechaAsignacion, DateTimeKind.Local);
                        }
                        else
                        {
                            fechaAsignacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error parseando fecha Creado '{i.Creados}' para ticket {i.Numero}: {ex.Message}");
                    }

                    // 🔹 Crear asignación consultor
                    TicketConsultorAsignacionInsertDto? ticketConsultorInsertDto = null;
                    try
                    {
                        if (consultor != null)
                        {
                            ticketConsultorInsertDto = new TicketConsultorAsignacionInsertDto
                            {
                                IdConsultor = consultor.Id,
                                IdTipoActividad = ObtenerTipoActividadPorCodigo(AppConstants.TipoActividad.AnalisisDeRequisitos),
                                FechaAsignacion = fechaAsignacion,
                                FechaDesasignacion = fechaAsignacion
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error creando TicketConsultorAsignacionInsertDto para ticket {i.Numero}: {ex.Message}");
                    }

                    // 🔹 Construir TicketInsertDto
                    return new TicketInsertMasivoDto
                    {
                        CodTicketInterno = i.Numero,
                        Titulo = i.BreveDescripcion,
                        FechaSolicitud = fechaAsignacion,
                        IdTipoTicket = ObtenerTipoTicketPorCodigo(AppConstants.TipoTicket.Incidencia),
                        IdEstadoTicket = MapearEstado(i.Estado),
                        IdEmpresa = _empresaDto.Id,
                        IdUsuarioResponsableCliente = _empresaDto.IdUser,
                        IdPrioridad = MapearPrioridad(i.Prioridad),
                        Descripcion = i.Descripcion,
                        UsuarioCreacion = i.Solicitante,
                        IdGestor = (int)_empresaDto.IdGestor,
                        EsCargaMasiva = true,
                        DatosCargaMasiva = System.Text.Json.JsonSerializer.Serialize(i, new JsonSerializerOptions { WriteIndented = true }),
                        ConsultorAsignaciones = ticketConsultorInsertDto != null
                            ? new List<TicketConsultorAsignacionInsertDto> { ticketConsultorInsertDto }
                            : new List<TicketConsultorAsignacionInsertDto>()
                    };
                }
                catch (Exception exInner)
                {
                    Console.WriteLine($"❌ Error procesando ticket '{i.Numero}': {exInner.Message}");
                    throw;
                }
            }).ToList();

        // ✅ Filtrar duplicados antes de insertar
        var nuevosTickets = ticketsInsertDto
            .Where(t => !_listaTicketsExistentes.Any(e => e.CodTicketInterno == t.CodTicketInterno))
            .ToList();

        if (!nuevosTickets.Any())
        {
            Console.WriteLine("⚠️ No hay tickets nuevos para insertar, todos ya existen en la base de datos.");
            return datos;
        }

        // ✅ Insertar solo los nuevos
        await CreateMasivoAsync(nuevosTickets);
        return datos;
    }
    private async Task<List<Dictionary<string, string>>> InsertarRequerimientosAlicorpAsync(ISheet sheet)
    {
        var datos = new List<Dictionary<string, string>>();

        var headerRow = sheet.GetRow(0);
        int colCount = headerRow.LastCellNum;

        for (int rowIdx = 1; rowIdx <= sheet.LastRowNum; rowIdx++)
        {
            var row = sheet.GetRow(rowIdx);
            if (row == null) continue;

            var fila = new Dictionary<string, string>();
            for (int colIdx = 0; colIdx < colCount; colIdx++)
            {
                string header = headerRow.GetCell(colIdx)?.ToString() ?? $"Column{colIdx}";
                string value = row.GetCell(colIdx)?.ToString() ?? "";
                fila[header] = value;
            }
            datos.Add(fila);
        }

        var incidentesAlicorpDto = datos.Select(d => new CargaMasivaRequerimientosAlicorpDto
        {
            Numero = d.ContainsKey("Número") ? d["Número"] : "",
            Solicitante = d.ContainsKey("Solicitante") ? d["Solicitante"] : "",
            UsuarioAfectado = d.ContainsKey("Usuario Afectado") ? d["Usuario Afectado"] : "",
            Estado = d.ContainsKey("Estado") ? d["Estado"] : "",
            MotivosParaPonerEnEspera = d.ContainsKey("Motivos para poner en espera") ? d["Motivos para poner en espera"] : "",
            Canal = d.ContainsKey("Canal") ? d["Canal"] : "",
            Prioridad = d.ContainsKey("Prioridad") ? d["Prioridad"] : "",
            Elemento = d.ContainsKey("Elemento") ? d["Elemento"] : "",
            Servicio = d.ContainsKey("Servicio") ? d["Servicio"] : "",
            Categoria1 = d.ContainsKey("Categoría 1") ? d["Categoría 1"] : "",
            Categoria2 = d.ContainsKey("Categoría 2") ? d["Categoría 2"] : "",
            Categoria3 = d.ContainsKey("Categoría 3") ? d["Categoría 3"] : "",
            BreveDescripcion = d.ContainsKey("Breve descripción") ? d["Breve descripción"] : "",
            Descripcion = d.ContainsKey("Descripción") ? d["Descripción"] : "",
            Sede = d.ContainsKey("Sede") ? d["Sede"] : "",
            GrupoDeAsignacion = d.ContainsKey("Grupo de asignación") ? d["Grupo de asignación"] : "",
            AsignadoA = d.ContainsKey("Asignado a") ? d["Asignado a"] : "",
            TicketExterno = d.ContainsKey("Ticket externo") ? d["Ticket externo"] : "",
            Creados = d.ContainsKey("Creados") ? d["Creados"] : "",
            CreadosPor = d.ContainsKey("Creados por") ? d["Creados por"] : "",
            Actualizados = d.ContainsKey("Actualizados") ? d["Actualizados"] : "",
            ActualizadoPor = d.ContainsKey("Actualizado por") ? d["Actualizado por"] : "",
            NivelFuncional = d.ContainsKey("Nivel Funcional") ? d["Nivel Funcional"] : ""
        }).ToList();
        var ticketsInsertDto = incidentesAlicorpDto.Select(i =>
        {
            try
            {
                // 🔹 Revisar campos importantes antes de usar .Value
                if (_empresaDto == null)
                    throw new Exception($"_empresaDto es null para ticket {i.Numero}");
                // 🔹 Buscar consultor
                var consultor = BuscarConsultorPorNombreCompleto(i.AsignadoA);

                // 🔹 Parsear fecha
                DateTime fechaAsignacion;
                try
                {
                    if (!string.IsNullOrWhiteSpace(i.Creados))
                    {
                        bool parseoExitoso = DateTime.TryParseExact(
                            i.Creados.Trim(),
                            new[] { "dd-MMM-yyyy", "dd/MM/yyyy HH:mm:ss" }, // múltiples formatos
                            new CultureInfo("es-PE"),
                            DateTimeStyles.None,
                            out fechaAsignacion
                        );

                        if (!parseoExitoso)
                            fechaAsignacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                        else
                            fechaAsignacion = DateTime.SpecifyKind(fechaAsignacion, DateTimeKind.Local);
                    }
                    else
                    {
                        fechaAsignacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error parseando fecha Creado '{i.Creados}' para ticket {i.Numero}: {ex.Message}");
                }

                // 🔹 Crear asignación consultor
                TicketConsultorAsignacionInsertDto? ticketConsultorInsertDto = null;
                try
                {
                    if (consultor != null)
                    {
                        ticketConsultorInsertDto = new TicketConsultorAsignacionInsertDto
                        {
                            IdConsultor = consultor.Id,
                            IdTipoActividad = ObtenerTipoActividadPorCodigo(AppConstants.TipoActividad.AnalisisDeRequisitos),
                            FechaAsignacion = fechaAsignacion,
                            FechaDesasignacion = fechaAsignacion
                        };
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error creando TicketConsultorAsignacionInsertDto para ticket {i.Numero}: {ex.Message}");
                }

                // 🔹 Construir TicketInsertDto
                return new TicketInsertMasivoDto
                {
                    CodTicketInterno = i.Numero,
                    Titulo = i.BreveDescripcion,
                    FechaSolicitud = fechaAsignacion,
                    IdTipoTicket = ObtenerTipoTicketPorCodigo(AppConstants.TipoTicket.Requerimiento),
                    IdEstadoTicket = MapearEstado(i.Estado),
                    IdEmpresa = _empresaDto.Id,
                    IdUsuarioResponsableCliente = _empresaDto.IdUser,
                    IdPrioridad = MapearPrioridad(i.Prioridad),
                    Descripcion = i.Descripcion,
                    UsuarioCreacion = i.Solicitante,
                    IdGestor = (int)_empresaDto.IdGestor,
                    EsCargaMasiva = true,
                    DatosCargaMasiva = System.Text.Json.JsonSerializer.Serialize(i, new JsonSerializerOptions { WriteIndented = true }),
                    ConsultorAsignaciones = ticketConsultorInsertDto != null
                        ? new List<TicketConsultorAsignacionInsertDto> { ticketConsultorInsertDto }
                        : new List<TicketConsultorAsignacionInsertDto>()
                };
            }
            catch (Exception exInner)
            {
                Console.WriteLine($"❌ Error procesando ticket '{i.Numero}': {exInner.Message}");
                throw;
            }
        }).ToList();

        // ✅ Filtrar duplicados antes de insertar
        var nuevosTickets = ticketsInsertDto
            .Where(t => !_listaTicketsExistentes.Any(e => e.CodTicketInterno == t.CodTicketInterno))
            .ToList();

        if (!nuevosTickets.Any())
        {
            Console.WriteLine("⚠️ No hay tickets nuevos para insertar, todos ya existen en la base de datos.");
            return datos;
        }

        // ✅ Insertar solo los nuevos
        await CreateMasivoAsync(nuevosTickets);
        return datos;
    }
    private async Task<List<Dictionary<string, string>>> InsertarIncidentesExceliaAsync(ISheet sheet)
    {
        var datos = new List<Dictionary<string, string>>();

        var headerRow = sheet.GetRow(0);
        int colCount = headerRow.LastCellNum;

        for (int rowIdx = 1; rowIdx <= sheet.LastRowNum; rowIdx++)
        {
            var row = sheet.GetRow(rowIdx);
            if (row == null) continue;

            var fila = new Dictionary<string, string>();
            for (int colIdx = 0; colIdx < colCount; colIdx++)
            {
                string header = headerRow.GetCell(colIdx)?.ToString() ?? $"Column{colIdx}";
                string value = row.GetCell(colIdx)?.ToString() ?? "";
                fila[header] = value;
            }
            datos.Add(fila);
        }

        // 🔹 Mapear los datos del Excel al DTO
        var incidentesNuevoDto = datos.Select(d => new CargaMasivaIncidentesExceliaDto
        {
            Ticket = d.GetValueOrDefault("Ticket", ""),
            Opened = d.GetValueOrDefault("Opened", ""),
            ShortDescription = d.GetValueOrDefault("Short description", ""),
            Caller = d.GetValueOrDefault("Caller", ""),
            Transaccion = d.GetValueOrDefault("Transacción", ""),
            Priority = d.GetValueOrDefault("Priority", ""),
            State = d.GetValueOrDefault("State", ""),
            Category1 = d.GetValueOrDefault("Category 1", ""),
            Category2 = d.GetValueOrDefault("Category 2", ""),
            AssignmentGroup = d.GetValueOrDefault("Assignment group", ""),
            AssignedTo = d.GetValueOrDefault("Assigned to", ""),
            Updated = d.GetValueOrDefault("Updated", ""),
            UpdatedBy = d.GetValueOrDefault("Updated by", ""),
            Category3 = d.GetValueOrDefault("Category 3", ""),
            CerradoPor = d.GetValueOrDefault("Cerrado por", ""),
            Company = d.GetValueOrDefault("Company", ""),
            CreatedBy = d.GetValueOrDefault("Created by", ""),
            Duration = d.GetValueOrDefault("Duration", ""),
            DurationOpened = d.GetValueOrDefault("Duration opened", ""),
            DurationPending = d.GetValueOrDefault("Duration pending", ""),
            DiasPendientes = d.GetValueOrDefault("Días pendientes", ""),
            FechaUltimoCambioDeGrupo = d.GetValueOrDefault("Fecha de ultimo cambio de grupo", ""),
            MadeSLA = d.GetValueOrDefault("Made SLA", ""),
            NotasDeTrabajo = d.GetValueOrDefault("Notas de trabajo", ""),
            NotasDeResolucion = d.GetValueOrDefault("Notas de resolución", ""),
            ResolveTime = d.GetValueOrDefault("Resolve time", ""),
            ResolvedBy = d.GetValueOrDefault("Resolved by", ""),
            TimeWorked = d.GetValueOrDefault("Time worked", ""),
            Urgency = d.GetValueOrDefault("Urgency", ""),
            UsuarioSolicitante = d.GetValueOrDefault("Usuario Solicitante", ""),
            Escalation = d.GetValueOrDefault("Escalation", ""),
            MotivoDePendiente = d.GetValueOrDefault("Motivo de pendiente", ""),
            Domain = d.GetValueOrDefault("Domain", ""),
            Closed = d.GetValueOrDefault("Closed", ""),
            Impact = d.GetValueOrDefault("Impact", ""),
            MayorIncident = d.GetValueOrDefault("Mayor incident", ""),
            Severity = d.GetValueOrDefault("Severity", ""),
            CopiaDeCategoria1 = d.GetValueOrDefault("Copia de Categoría 1", ""),
            BusinessService = d.GetValueOrDefault("Business service", ""),
            DueDate = d.GetValueOrDefault("Due date", ""),
            SLA = d.GetValueOrDefault("SLA", ""),
            SLADue = d.GetValueOrDefault("SLA due", ""),
            RequestUser = d.GetValueOrDefault("Request User", ""),
            OpenedBy = d.GetValueOrDefault("Opened by", ""),
            SAPSociety = d.GetValueOrDefault("SAP Society", ""),
            CodigoDeCierre = d.GetValueOrDefault("Código de cierre", ""),
            Resolved = d.GetValueOrDefault("Resolved", ""),
            NivelFuncional = d.GetValueOrDefault("Nivel Funcional", "")
        }).ToList();

        // 🔹 Convertir DTO → TicketInsertMasivoDto
        var ticketsInsertDto = incidentesNuevoDto.Select(i =>
        {
            try
            {
                if (_empresaDto == null)
                    throw new Exception($"_empresaDto es null para ticket {i.Ticket}");

                // 🔹 Buscar consultor
                var consultor = BuscarConsultorPorNombreCompleto(i.AssignedTo);

                // 🔹 Parsear fecha de creación
                DateTime fechaCreacion;
                if (!DateTime.TryParse(i.Opened, new CultureInfo("es-PE"), DateTimeStyles.None, out fechaCreacion))
                    fechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

                // 🔹 Crear asignación consultor
                TicketConsultorAsignacionInsertDto? ticketConsultorInsertDto = null;
                if (consultor != null)
                {
                    ticketConsultorInsertDto = new TicketConsultorAsignacionInsertDto
                    {
                        IdConsultor = consultor.Id,
                        IdTipoActividad = ObtenerTipoActividadPorCodigo(AppConstants.TipoActividad.AnalisisDeRequisitos),
                        FechaAsignacion = fechaCreacion,
                        FechaDesasignacion = fechaCreacion
                    };
                }

                // 🔹 Construir TicketInsertDto
                return new TicketInsertMasivoDto
                {
                    CodTicketInterno = i.Ticket,
                    Titulo = i.ShortDescription,
                    FechaSolicitud = fechaCreacion,
                    IdTipoTicket = ObtenerTipoTicketPorCodigo(AppConstants.TipoTicket.Incidencia),
                    IdEstadoTicket = MapearEstado(i.State),
                    IdEmpresa = _empresaDto.Id,
                    IdUsuarioResponsableCliente = _empresaDto.IdUser,
                    IdPrioridad = MapearPrioridad(i.Priority),
                    Descripcion = i.NotasDeResolucion,
                    UsuarioCreacion = i.CreatedBy,
                    IdGestor = (int)_empresaDto.IdGestor,
                    EsCargaMasiva = true,
                    DatosCargaMasiva = System.Text.Json.JsonSerializer.Serialize(i, new JsonSerializerOptions { WriteIndented = true }),
                    ConsultorAsignaciones = ticketConsultorInsertDto != null
                        ? new List<TicketConsultorAsignacionInsertDto> { ticketConsultorInsertDto }
                        : new List<TicketConsultorAsignacionInsertDto>()
                };
            }
            catch (Exception exInner)
            {
                Console.WriteLine($"❌ Error procesando ticket '{i.Ticket}': {exInner.Message}");
                throw;
            }
        }).ToList();

        // ✅ Filtrar duplicados antes de insertar
        var nuevosTickets = ticketsInsertDto
            .Where(t => !_listaTicketsExistentes.Any(e => e.CodTicketInterno == t.CodTicketInterno))
            .ToList();

        if (!nuevosTickets.Any())
        {
            Console.WriteLine("⚠️ No hay tickets nuevos para insertar, todos ya existen en la base de datos.");
            return datos;
        }

        // ✅ Insertar solo los nuevos
        await CreateMasivoAsync(nuevosTickets);
        return datos;
    }
    private async Task<List<Dictionary<string, string>>> InsertarTicketsRansaAsync(ISheet sheet)
    {
        var datos = new List<Dictionary<string, string>>();

        var headerRow = sheet.GetRow(0);
        int colCount = headerRow.LastCellNum;

        for (int rowIdx = 1; rowIdx <= sheet.LastRowNum; rowIdx++)
        {
            var row = sheet.GetRow(rowIdx);
            if (row == null) continue;

            var fila = new Dictionary<string, string>();
            for (int colIdx = 0; colIdx < colCount; colIdx++)
            {
                string header = headerRow.GetCell(colIdx)?.ToString() ?? $"Column{colIdx}";
                string value = row.GetCell(colIdx)?.ToString() ?? "";
                fila[header] = value;
            }
            datos.Add(fila);
        }

        // 🔹 Mapear los datos del Excel al DTO
        var ticketsNuevoDto = datos.Select(d => new CargaMasivaTicketsRansaDto
        {
            IdTicket = d.GetValueOrDefault("ID del ticket", ""),
            TipoDeTicket = d.GetValueOrDefault("Tipo de ticket", ""),
            TicketRelacionado = d.GetValueOrDefault("Ticket Relacionado", ""),
            Fuente = d.GetValueOrDefault("Fuente", ""),
            Solicitante = d.GetValueOrDefault("Solicitante", ""),
            CorreoElectronicoSolicitante = d.GetValueOrDefault("Correo electrónico del solicitante", ""),
            UsuarioAfectado = d.GetValueOrDefault("Usuario Afectado", ""),
            CorreoUsuarioAfectado = d.GetValueOrDefault("Correo Usuario Afectado", ""),
            VIP = d.GetValueOrDefault("VIP", ""),
            ServiciosAfectados = d.GetValueOrDefault("Servicios afectados", ""),
            Clase = d.GetValueOrDefault("Clase", ""),
            Categoria = d.GetValueOrDefault("Categoría", ""),
            Tipo = d.GetValueOrDefault("Tipo", ""),
            Elemento = d.GetValueOrDefault("Elemento", ""),
            Impacto = d.GetValueOrDefault("Impacto", ""),
            Urgencia = d.GetValueOrDefault("Urgencia", ""),
            Prioridad = d.GetValueOrDefault("Prioridad", ""),
            FechaDeCreacion = d.GetValueOrDefault("Fecha de creación", ""),
            Mes = d.GetValueOrDefault("MES", ""),
            FechaDeResolucion = d.GetValueOrDefault("Fecha de resolución", ""),
            FechaDeCierre = d.GetValueOrDefault("Fecha de cierre", ""),
            AsignarAlGrupo = d.GetValueOrDefault("Asignar al grupo", ""),
            AsignarAlIndividuo = d.GetValueOrDefault("Asignar al individuo", ""),
            AutorDeLaCreacion = d.GetValueOrDefault("Autor de la creación", ""),
            Estado = d.GetValueOrDefault("Estado", ""),
            Motivo = d.GetValueOrDefault("Motivo", ""),
            AntiguedadDelTicketDias = d.GetValueOrDefault("Antigüedad del ticket (días)", ""),
            Descripcion = d.GetValueOrDefault("Descripción", ""),
            Detalles = d.GetValueOrDefault("Detalles", ""),
            UbicacionId139 = d.GetValueOrDefault("ubicación_id139", ""),
            CodigoDeCierre = d.GetValueOrDefault("Código de Cierre", ""),
            CandidatoProblema = d.GetValueOrDefault("Candidato Problema?", "")
        }).ToList();

        // 🔹 Convertir DTO → TicketInsertMasivoDto
        var ticketsInsertDto = ticketsNuevoDto.Select(i =>
        {
            try
            {
                if (_empresaDto == null)
                    throw new Exception($"_empresaDto es null para ticket {i.IdTicket}");

                // 🔹 Buscar consultor
                var consultor = BuscarConsultorPorNombreCompleto(i.AsignarAlIndividuo);

                // 🔹 Parsear fecha de creación
                DateTime fechaCreacion;
                if (!DateTime.TryParse(i.FechaDeCreacion, new CultureInfo("es-PE"), DateTimeStyles.None, out fechaCreacion))
                    fechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

                // 🔹 Crear asignación consultor
                TicketConsultorAsignacionInsertDto? ticketConsultorInsertDto = null;
                if (consultor != null)
                {
                    ticketConsultorInsertDto = new TicketConsultorAsignacionInsertDto
                    {
                        IdConsultor = consultor.Id,
                        IdTipoActividad = ObtenerTipoActividadPorCodigo(AppConstants.TipoActividad.AnalisisDeRequisitos),
                        FechaAsignacion = fechaCreacion,
                        FechaDesasignacion = fechaCreacion
                    };
                }

                // 🔹 Construir TicketInsertMasivoDto (solo los campos definidos)
                return new TicketInsertMasivoDto
                {
                    CodTicketInterno = i.IdTicket,
                    Titulo = i.Descripcion,
                    FechaSolicitud = fechaCreacion,
                    IdTipoTicket = ObtenerTipoTicketPorCodigo(AppConstants.TipoTicket.Incidencia),
                    IdEstadoTicket = MapearEstado(i.Estado),
                    IdEmpresa = _empresaDto.Id,
                    IdUsuarioResponsableCliente = _empresaDto.IdUser,
                    IdPrioridad = MapearPrioridad(i.Prioridad),
                    Descripcion = i.Detalles,
                    UsuarioCreacion = i.AutorDeLaCreacion,
                    IdGestor = (int)_empresaDto.IdGestor,
                    EsCargaMasiva = true,
                    DatosCargaMasiva = System.Text.Json.JsonSerializer.Serialize(i, new JsonSerializerOptions { WriteIndented = true }),
                    ConsultorAsignaciones = ticketConsultorInsertDto != null
                        ? new List<TicketConsultorAsignacionInsertDto> { ticketConsultorInsertDto }
                        : new List<TicketConsultorAsignacionInsertDto>()
                };
            }
            catch (Exception exInner)
            {
                Console.WriteLine($"❌ Error procesando ticket '{i.IdTicket}': {exInner.Message}");
                throw;
            }
        }).ToList();

        // ✅ Filtrar duplicados antes de insertar
        var nuevosTickets = ticketsInsertDto
            .Where(t => !_listaTicketsExistentes.Any(e => e.CodTicketInterno == t.CodTicketInterno))
            .ToList();

        if (!nuevosTickets.Any())
        {
            Console.WriteLine("⚠️ No hay tickets nuevos para insertar, todos ya existen en la base de datos.");
            return datos;
        }

        // ✅ Insertar solo los nuevos
        await CreateMasivoAsync(nuevosTickets);
        return datos;
    }

    public async Task<List<Ticket>> CreateMasivoAsync(List<TicketInsertMasivoDto> insertDtos)
    {
        if (insertDtos == null || !insertDtos.Any())
            return new List<Ticket>();

        // 🔹 Obtener el código base del tipo de ticket (una sola vez)
        string codigoTipoTicket = (await _parametroRepository.GetByIdAsync(insertDtos.First().IdTipoTicket)).Codigo;

        // 🔹 Obtener el último Id de ticket ya existente
        int ultimoId = (await _ticketRepository.GetAllAsync()).DefaultIfEmpty().Max(t => t?.Id ?? 0);

        // 🔹 Generar tickets con código incremental y mapear propiedades
        var tickets = insertDtos.Select((insertDto, index) =>
        {
            int nextId = ultimoId + index + 1;
            string fechaHora = DateTime.Now.ToString("yyyyMMddHHmmss");
            string codTicket = $"{codigoTipoTicket}-{nextId}-{fechaHora}";

            return new Ticket
            {
                CodTicketInterno = insertDto.CodTicketInterno,
                Titulo = insertDto.Titulo,
                FechaSolicitud = DateTime.SpecifyKind(insertDto.FechaSolicitud, DateTimeKind.Local),
                IdTipoTicket = insertDto.IdTipoTicket,
                IdEstadoTicket = insertDto.IdEstadoTicket,
                IdEmpresa = insertDto.IdEmpresa,
                IdUsuarioResponsableCliente = insertDto.IdUsuarioResponsableCliente ?? 0,
                IdPrioridad = insertDto.IdPrioridad,
                Descripcion = insertDto.Descripcion,
                UsuarioCreacion = insertDto.UsuarioCreacion?.Substring(0, Math.Min(50, insertDto.UsuarioCreacion.Length)),
                IdGestor = insertDto.IdGestor,
                Activo = true,
                FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
                UrlArchivos = null,
                CodTicket = codTicket,
                EsCargaMasiva = true,
                DatosCargaMasiva = insertDto.DatosCargaMasiva,
                // 🔹 Mapear asignaciones de consultores sin IdTicket aún
                ConsultorAsignaciones = insertDto.ConsultorAsignaciones?
                    .Select(a => new TicketConsultorAsignacion
                    {
                        Id = 0, // evita duplicados al insertar
                        IdConsultor = a.IdConsultor,
                        IdTipoActividad = a.IdTipoActividad,
                        FechaAsignacion = DateTime.SpecifyKind(a.FechaAsignacion, DateTimeKind.Local),
                        FechaDesasignacion = DateTime.SpecifyKind(a.FechaDesasignacion, DateTimeKind.Local),
                        Activo = true
                    }).ToList() ?? new List<TicketConsultorAsignacion>()
            };
        }).ToList();

        // 🔹 Guardar todos los tickets en batch
        var ticketsGuardados = await _ticketRepository.CreateRangeAsync(tickets);

        //// 🔹 Asignar IdTicket a las asignaciones y guardarlas en batch
        //var todasAsignaciones = ticketsGuardados
        //    .SelectMany(t => t.ConsultorAsignaciones.Select(ca =>
        //    {
        //        ca.IdTicket = t.Id;
        //        return ca;
        //    }))
        //    .ToList();

        //if (todasAsignaciones.Any())
        //    await _consultorAsignacionRepository.CreateRangeAsync(todasAsignaciones);

        return ticketsGuardados;
    }


}
