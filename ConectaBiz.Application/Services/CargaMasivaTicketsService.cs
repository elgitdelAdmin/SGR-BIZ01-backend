using AutoMapper;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Constants;
using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Globalization;
using System.Text;
using System.Text.Json;
using static ConectaBiz.Domain.Constants.AppConstants;
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
    private readonly ISubFrenteRepository _subFrenteRepository;
    private readonly IMapper _mapper;

    // 🔹 Variables para cachear los datos que cargamos en ProcesarExcelAsync
    private IEnumerable<Parametro> _listaTipoTicket;
    private IEnumerable<Parametro> _listaSubTipoTicket;
    private IEnumerable<Parametro> _listaEstados;
    private IEnumerable<Parametro> _listaPrioridades;
    private EmpresaDto _empresaDto;
    private IEnumerable<ConsultorDto> _listaConsultores;
    private IEnumerable<Parametro> _listaParametros;
    private IEnumerable<Parametro> _listaTipoActividad;
    private IEnumerable<Ticket> _listaTicketsExistentes;
    private IEnumerable<SubFrente> _listaSubFrentes;
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
        ISubFrenteRepository subFrenteRepository,
        IMapper mapper)
    {
        _empresaRepository = empresaRepository;
        _consultorAsignacionRepository = consultorAsignacionRepository;
        _cargaMasivaTicketsRepository = cargaMasivaTicketsRepository;
        _ticketRepository = ticketRepository;
        _parametroRepository = parametroRepository;
        _empresaService = empresaService;
        _consultorService = consultorService;
        _subFrenteRepository = subFrenteRepository;
        _mapper = mapper;
    }

    public async Task<List<Dictionary<string, string>>> ProcesarExcelAsync(Stream stream, string tipo)
    {
        try
        {
            // 🔹 Determinar empresa según tipo
            string numDocContribuyenteEmpresa = tipo switch
            {
                //AppConstants.TipoCargaMasiva.IncidentesAlicorp => AppConstants.Empresas.AlicorpNumDocContribuyente,
                //AppConstants.TipoCargaMasiva.RequerimientosAlicorp => AppConstants.Empresas.AlicorpNumDocContribuyente,
                AppConstants.TipoCargaMasiva.TicketsExcelia => AppConstants.Empresas.ExceliaNumDocContribuyente,
                AppConstants.TipoCargaMasiva.TicketsRansa => AppConstants.Empresas.RansaNumDocContribuyente,
                AppConstants.TipoCargaMasiva.TicketsIasa => AppConstants.Empresas.IasaNumDocContribuyente,
                AppConstants.TipoCargaMasiva.TicketsAlicorp => AppConstants.Empresas.AlicorpNumDocContribuyente,
                _ => throw new Exception("Tipo de carga masiva no soportado")
            };

            _tipoCarga = tipo;

            // 🔹 Cargar todos los datos necesarios
            await InicializarDatosAsync(numDocContribuyenteEmpresa);

            // 🔹 Determinar empresa según tipo
            _tipoTicket = tipo switch
            {
                //AppConstants.TipoCargaMasiva.IncidentesAlicorp => ObtenerTipoTicketPorCodigo(AppConstants.TipoTicket.Incidencia) ,
                //AppConstants.TipoCargaMasiva.RequerimientosAlicorp => ObtenerTipoTicketPorCodigo(AppConstants.TipoTicket.Requerimiento),
                AppConstants.TipoCargaMasiva.TicketsAlicorp => null,
                AppConstants.TipoCargaMasiva.TicketsExcelia => null,
                AppConstants.TipoCargaMasiva.TicketsRansa => null,
                AppConstants.TipoCargaMasiva.TicketsIasa => null,
                _ => throw new Exception("Tipo de carga masiva no soportado")
            };


            // Abrir Excel
            XSSFWorkbook workbook = new XSSFWorkbook(stream);
            var sheet = workbook.GetSheetAt(0);
            if (sheet == null) return new List<Dictionary<string, string>>();
            return tipo switch
            {

                //AppConstants.TipoCargaMasiva.IncidentesAlicorp => await InsertarGenericoAsync(sheet),
                //AppConstants.TipoCargaMasiva.RequerimientosAlicorp => await InsertarGenericoAsync(sheet),
                AppConstants.TipoCargaMasiva.TicketsAlicorp => await InsertarGenericoAsync(sheet),
                AppConstants.TipoCargaMasiva.TicketsExcelia => await InsertarGenericoAsync(sheet),
                AppConstants.TipoCargaMasiva.TicketsRansa => await InsertarGenericoAsync(sheet),
                AppConstants.TipoCargaMasiva.TicketsIasa => await InsertarGenericoAsync(sheet),
                _ => throw new Exception("Tipo de carga masiva no soportado")
            };
        }
        catch (Exception ex)
        {
            throw;
        }
    }


    private async Task InicializarDatosAsync(string numDocContribuyenteEmpresa)
    {
        _listaParametros = await _parametroRepository.GetAllAsync();
        _listaTipoTicket = _listaParametros.Where(p => p.TipoParametro == AppConstants.TiposParametros.TipoTicket).ToList();
        _listaSubTipoTicket = _listaParametros.Where(p => p.TipoParametro == AppConstants.TiposParametros.SubTipoTicket).ToList();
        _listaEstados = _listaParametros.Where(p => p.TipoParametro == AppConstants.TiposParametros.EstadoTicket).ToList();
        _listaPrioridades = _listaParametros.Where(p => p.TipoParametro == AppConstants.TiposParametros.Prioridad).ToList();
        _listaTipoActividad = _listaParametros.Where(p => p.TipoParametro == AppConstants.TiposParametros.TipoActividad).ToList();
        _empresaDto = await _empresaService.GetByNumDocContribuyenteAsync(numDocContribuyenteEmpresa,AppConstants.Socios.CstiNumDocContribuyente);
        _listaTicketsExistentes = await _ticketRepository.GetByNumContribuyenteSocioEmpAsync(AppConstants.Socios.CstiNumDocContribuyente, numDocContribuyenteEmpresa);
        _listaConsultores = await _consultorService.GetByNumDocContribuyenteSocioAsync(AppConstants.Socios.CstiNumDocContribuyente);
        _listaSubFrentes = await _subFrenteRepository.GetActiveAsync();
    }

    // Obtener consultor
    private ConsultorDto? BuscarConsultorPorNombreCompleto(string nombreCompleto)
    {
        if (string.IsNullOrWhiteSpace(nombreCompleto) || _listaConsultores == null)
            return null;

        // 🔹 Lógica original
        var partes = nombreCompleto
            .Split(" ", StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim().ToLower())
            .ToList();

        // 🔹 Palabras prohibidas o que deben eliminarse
        var palabrasNoDeseadas = new List<string>
        {
            "csti_facturación",
            "csti_finanzas",
            "csti_compras",
            "csti_rrhh",
            "csti_abap",
            "csti_pp",
            "csti_controlling",
            "csti_" // este último elimina cualquier palabra que empiece con "csti_"
        };

        // 🔹 Filtrar las palabras no deseadas
        partes = partes
            .Where(p => !palabrasNoDeseadas.Any(nd => p.Contains(nd)))
            .ToList();

        // 🔹 Si después de limpiar no queda nada, retornar null
        if (partes.Count == 0)
            return null;

        foreach (var consultor in _listaConsultores)
        {
            var persona = consultor.Persona;
            var nombrePersona = $"{persona.Nombres} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".ToLower();

            bool coincide = partes.All(p => nombrePersona.Contains(p));
            if (coincide)
                return consultor;
        }
        // 🔹 Segunda lógica: manejar comas y dividir en palabras
        var partesPorComa = nombreCompleto
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .SelectMany(fragmento => fragmento.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Select(p => p.Trim().ToLower())
            .ToList();

        // Buscar el consultor con más coincidencias de palabras
        ConsultorDto? mejorCoincidencia = null;
        int maxCoincidencias = 0;

        foreach (var consultor in _listaConsultores)
        {
            var persona = consultor.Persona;
            var nombrePersona = $"{persona.Nombres} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".ToLower();

            int coincidencias = partesPorComa.Count(p => nombrePersona.Contains(p));

            // Si tiene más coincidencias que el actual, lo guardamos
            if (coincidencias > maxCoincidencias)
            {
                maxCoincidencias = coincidencias;
                mejorCoincidencia = consultor;
            }
        }
        // Devolvemos el consultor con más coincidencias (si hay al menos una)
        if (maxCoincidencias > 0)
            return mejorCoincidencia;


        // 🔹 Tercera lógica: comparar sin tildes
        string QuitarTildes(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return texto;
            var normalized = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString();
        }

        var partesSinTilde = partesPorComa.Select(p => QuitarTildes(p)).ToList();

        foreach (var consultor in _listaConsultores)
        {
            var persona = consultor.Persona;
            var nombrePersona = QuitarTildes($"{persona.Nombres} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".ToLower());

            bool coincide = partesSinTilde.All(p => nombrePersona.Contains(p));
            if (coincide)
                return consultor;
        }

        // 🔹 Lógica 4: eliminar palabras con "_ext", "ext_", o que sean exactamente "_ext"
        var partesSinExt = nombreCompleto
            .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(p =>
            {
                var palabra = p.Trim();
                return !palabra.Equals("_ext", StringComparison.OrdinalIgnoreCase)
                       && !palabra.Contains("_ext", StringComparison.OrdinalIgnoreCase)
                       && !palabra.Contains("ext_", StringComparison.OrdinalIgnoreCase);
            })
            .Select(p => p.Trim().ToLower())
            .ToList();

        if (partesSinExt.Count > 0)
        {
            foreach (var consultor in _listaConsultores)
            {
                var persona = consultor.Persona;
                var nombrePersona = $"{persona.Nombres} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".ToLower();

                bool coincide = partesSinExt.All(p => nombrePersona.Contains(p));
                if (coincide)
                    return consultor;
            }
        }

        return null; // no encontró coincidencia
    }


    // 🔹 Mapeo Tipo Ticket
    private int ObtenerTipoActividadPorCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return 0;
        var tipoActividad = _listaTipoActividad.FirstOrDefault(t => t.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase));
        return tipoActividad != null ? tipoActividad.Id : 0;
    }

    private string LimpiarCodTicketInterno(string codTicket)
    {
        if (string.IsNullOrWhiteSpace(codTicket)) return string.Empty;

        return codTicket
            .Replace("Solicitud", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Incidente", "", StringComparison.OrdinalIgnoreCase)
            .Replace(" ", "")
            .Trim();
    }

    // 🔹 Mapeo Tipo Ticket
    private int LogicaObtenerSubTipoTicketPorCodigo(string codigo)
    {
        int idTipoTicket = 0;

        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("El código del ticket no puede estar vacío.", nameof(codigo));

        codigo = codigo.Trim();

        if (_tipoCarga == AppConstants.TipoCargaMasiva.TicketsExcelia)
        {
            var codigoCorto = codigo.Length >= 3 ? codigo.Substring(0, 3) : codigo;

            idTipoTicket = codigoCorto switch
            {
                AppConstants.TipoCargaMasiva.TipoCargaMasivaTicketExcelia.Solicitud => ObtenerSubTipoTicketPorCodigo(AppConstants.SubtipoTicket.MesaDeAyuda.Requerimiento),
                AppConstants.TipoCargaMasiva.TipoCargaMasivaTicketExcelia.Incidentes => ObtenerSubTipoTicketPorCodigo(AppConstants.SubtipoTicket.MesaDeAyuda.Incidencia),
                _ => throw new Exception("LogicaObtenerSubTipoTicketPorCodigo() no soportado")
            };
        }
        else if (_tipoCarga == AppConstants.TipoCargaMasiva.TicketsRansa)
        {
            var codigoCorto = codigo.Length >= 3 ? codigo.Substring(0, 3) : codigo;

            idTipoTicket = codigoCorto switch
            {
                AppConstants.TipoCargaMasiva.TipoCargaMasivaTicketRansa.Requerimientos => ObtenerSubTipoTicketPorCodigo(AppConstants.SubtipoTicket.MesaDeAyuda.Requerimiento),
                AppConstants.TipoCargaMasiva.TipoCargaMasivaTicketRansa.Incidentes => ObtenerSubTipoTicketPorCodigo(AppConstants.SubtipoTicket.MesaDeAyuda.Incidencia),
                _ => throw new Exception("LogicaObtenerSubTipoTicketPorCodigo() no soportado")
            };
        }
        else if (_tipoCarga == AppConstants.TipoCargaMasiva.TicketsIasa)
        {
            var codigoCorto = codigo.Length >= 9 ? codigo.Substring(0, 9) : codigo;

            idTipoTicket = codigoCorto switch
            {
                AppConstants.TipoCargaMasiva.TipoCargaMasivaTicketIasa.Requerimientos => ObtenerSubTipoTicketPorCodigo(AppConstants.SubtipoTicket.MesaDeAyuda.Requerimiento),
                AppConstants.TipoCargaMasiva.TipoCargaMasivaTicketIasa.Incidentes => ObtenerSubTipoTicketPorCodigo(AppConstants.SubtipoTicket.MesaDeAyuda.Incidencia),
                _ => throw new Exception("LogicaObtenerSubTipoTicketPorCodigo() no soportado")
            };
        }
        else if (_tipoCarga == AppConstants.TipoCargaMasiva.TicketsAlicorp)
        {
            idTipoTicket = codigo switch
            {
                var c when c.StartsWith(AppConstants.TipoCargaMasiva.TipoCargaMasivaTicketAlicorp.Requerimientos)
                    => ObtenerSubTipoTicketPorCodigo(AppConstants.SubtipoTicket.MesaDeAyuda.Requerimiento),

                var c when c.StartsWith(AppConstants.TipoCargaMasiva.TipoCargaMasivaTicketAlicorp.Solicitud)
                    => ObtenerSubTipoTicketPorCodigo(AppConstants.SubtipoTicket.MesaDeAyuda.Requerimiento),

                var c when c.StartsWith(AppConstants.TipoCargaMasiva.TipoCargaMasivaTicketAlicorp.Incidentes)
                    => ObtenerSubTipoTicketPorCodigo(AppConstants.SubtipoTicket.MesaDeAyuda.Incidencia),

                _ => throw new Exception("LogicaObtenerSubTipoTicketPorCodigo() no soportado")
            };
        }

        else if (_tipoCarga == AppConstants.TipoCargaMasiva.RequerimientosAlicorp)
        {
            idTipoTicket = ObtenerSubTipoTicketPorCodigo(AppConstants.SubtipoTicket.MesaDeAyuda.Requerimiento);
        }
        else if (_tipoCarga == AppConstants.TipoCargaMasiva.IncidentesAlicorp)
        {
            idTipoTicket = ObtenerSubTipoTicketPorCodigo(AppConstants.SubtipoTicket.MesaDeAyuda.Incidencia);
        }
        else
        {
            throw new Exception("Tipo de carga no reconocido en LogicaObtenerSubTipoTicketPorCodigo()");
        }
        return idTipoTicket;
    }

    private int ObtenerSubTipoTicketPorCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return 0;
        var subTipoTicket = _listaSubTipoTicket.FirstOrDefault(t =>t.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase));
        return subTipoTicket != null ? subTipoTicket.Id : 0;
    }
    //private string ObtenerCodigoTicketPorId(int id)
    //{
    //    if (id <= 0) return string.Empty;
    //    var subTipoTicket = _listaSubTipoTicket.FirstOrDefault(t => t.Id == id);
    //    return subTipoTicket != null ? subTipoTicket.Codigo : string.Empty;
    //}
    private string ObtenerCodigoTicketPorId(int id)
    {
        if (id <= 0) return string.Empty;
        var tipoTicket = _listaTipoTicket.FirstOrDefault(t => t.Id == id);
        return tipoTicket != null ? tipoTicket.Codigo : string.Empty;
    }
    // 🔹 Mapeo Estado
    private int MapearEstado(string estadoRecibido)
    {
        if (string.IsNullOrWhiteSpace(estadoRecibido))
            return 0;

        string estadoNormalizado = estadoRecibido.Trim();
        var mapeo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // --- 🔹 Ransa ---
        if (_tipoCarga == AppConstants.TipoCargaMasiva.TicketsRansa)
        {
            mapeo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
           /* { "Queued", "PENDIENTE_ATENCION" },
            { "Closed", "CERRADO" }*/
            { "Queued", "CERRADO" },
            { "Closed", "CERRADO" }
        };
        }
        else if(_tipoCarga == AppConstants.TipoCargaMasiva.TicketsIasa)
        {
        mapeo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            /* { "Pending", "PENDIENTE_ATENCION" },
            { "En proceso", "EN_EJECUCION" },
            { "Por disponibilidad del usuario", "PENDIENTE_CLIENTE" }*/

            { "Pending", "CERRADO" },
            { "En proceso", "CERRADO" },
            { "Por disponibilidad del usuario", "CERRADO" }
        };
        }
        // --- 🔹 Otros casos ---
        else
        {
            mapeo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            /*{ "Aprobado", "APROBADO" },
            { "Cancelado", "CANCELADO" },
            { "Cerrado", "CERRADO" },
            { "Pendiente", "PENDIENTE_ATENCION" },
            { "Resuelto", "CERRADO" }*/

            { "Aprobado", "CERRADO" },
            { "Cancelado", "CERRADO" },
            { "Cerrado", "CERRADO" },
            { "Pendiente", "CERRADO" },
            { "Resuelto", "CERRADO" }
        };
        }

        // 🔹 Aplica el mapeo
        if (mapeo.TryGetValue(estadoNormalizado, out var codigoInterno))
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
        if (string.IsNullOrWhiteSpace(prioridadExcel))
            return 0;

        string nombrePrioridad = prioridadExcel.Trim(); // ✅ siempre inicializamos con el valor recibido
        var mapeoNombres = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // --- Ransa ---
        if (_tipoCarga == AppConstants.TipoCargaMasiva.TicketsRansa || _tipoCarga == AppConstants.TipoCargaMasiva.TicketsIasa)
        {
        mapeoNombres = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Medium", "Media" },
            { "Low", "Baja" },
            { "High", "Alta" },
            { "Critical", "Critica" }
        };
        }
        // --- Excelia ---
        else if (_tipoCarga == AppConstants.TipoCargaMasiva.TicketsExcelia)
        {
            mapeoNombres = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "3", "Baja" },
            { "2", "Media" },
            { "1", "Alta" }
        };
        }
        // --- Otros ---
        else
        {
            var partes = prioridadExcel.Split('-', 2);
            nombrePrioridad = partes.Length == 2 ? partes[1].Trim() : prioridadExcel.Trim();

            mapeoNombres = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Medio", "Media" },
            { "Baja", "Baja" },
            { "Alta", "Alta" },
            { "Crítica", "Crítica" }
        };
        }

        // 🔹 Aplica el mapeo (por ejemplo: "3" -> "Baja")
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
        if (headerRow == null) return datos;

        // ✅ Obtener TODAS las columnas con encabezado
        var columnas = new List<(int Index, string Header)>();
        for (int colIdx = 0; colIdx < headerRow.LastCellNum; colIdx++)
        {
            var headerCell = headerRow.GetCell(colIdx);
            string header = headerCell?.ToString()?.Trim() ?? "";

            if (!string.IsNullOrWhiteSpace(header))
            {
                columnas.Add((colIdx, header));
            }
        }

        if (columnas.Count == 0) return datos;
        
        // 🔹 LOG: Ver qué columnas detectamos realmente
        Console.WriteLine($"🔍 Columnas detectadas en Excel: {string.Join(", ", columnas.Select(c => c.Header))}");

        // ✅ DEFINIR COLUMNAS OBLIGATORIAS (ajusta según tus necesidades)
        // Nota: Agregamos ignore case al set para validación posterior si fuera necesario, 
        // pero la validación principal depende del diccionario de la fila.
        var columnasObligatorias = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
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

        var formatter = new DataFormatter();

        for (int rowIdx = 1; rowIdx <= sheet.LastRowNum; rowIdx++)
        {
            var row = sheet.GetRow(rowIdx);
            if (row == null) continue;

            // ✅ Verificar que la fila tenga contenido en al menos una celda
            bool tieneContenido = false;
            foreach (var (colIdx, _) in columnas)
            {
                var celda = row.GetCell(colIdx);
                if (celda != null && celda.CellType != CellType.Blank)
                {
                    string valor = "";
                    try
                    {
                        valor = formatter.FormatCellValue(celda)?.Trim() ?? "";
                    }
                    catch
                    {
                        valor = celda.ToString()?.Trim() ?? "";
                    }

                    if (!string.IsNullOrWhiteSpace(valor))
                    {
                        tieneContenido = true;
                        break;
                    }
                }
            }

            if (!tieneContenido) continue;

            // 🔹 Usar StringComparer.OrdinalIgnoreCase para que "codTicket" coincida con "CodTicket"
            var fila = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                // ✅ Procesar todas las columnas
                foreach (var (colIdx, header) in columnas)
                {
                    var dataCell = row.GetCell(colIdx);
                    string value = "";

                    if (dataCell != null && dataCell.CellType != CellType.Blank)
                    {
                        try
                        {
                            value = formatter.FormatCellValue(dataCell) ?? "";
                        }
                        catch
                        {
                            value = dataCell.ToString() ?? "";
                        }
                    }

                    fila[header] = value.Trim();
                }

                // ✅ VALIDAR que las columnas obligatorias tengan valor
                bool filaValida = true;
                var columnasVacias = new List<string>();

                foreach (var columnaObligatoria in columnasObligatorias)
                {
                    // Al usar un diccionario CaseInsensitive, podemos buscar directo
                    if (fila.TryGetValue(columnaObligatoria, out string valor))
                    {
                        if (string.IsNullOrWhiteSpace(valor))
                        {
                            filaValida = false;
                            columnasVacias.Add($"{columnaObligatoria} (VACÍO)");
                        }
                    }
                    else
                    {
                        // La columna obligatoria ni siquiera existe en el Excel
                        filaValida = false;
                        columnasVacias.Add($"{columnaObligatoria} (NO ENCONTRADA)");
                    }
                }

                // Validar GrupoAsignacion o GrupoAsignación
                string valorGrupo = fila.TryGetValue("GrupoAsignacion", out var g1) ? g1 
                                  : fila.TryGetValue("GrupoAsignación", out var g2) ? g2 
                                  : null;

                if (valorGrupo == null)
                {
                    filaValida = false;
                    columnasVacias.Add("GrupoAsignacion (NO ENCONTRADA)");
                }
                else if (string.IsNullOrWhiteSpace(valorGrupo))
                {
                    filaValida = false;
                    columnasVacias.Add("GrupoAsignacion (VACÍO)");
                }

                if (!filaValida)
                {
                    // 🔹 LOG: Detalle exacto de qué falló
                    Console.WriteLine($"⚠️ Fila {rowIdx} omitida. Faltan: {string.Join(", ", columnasVacias)}");
                    
                    // Opcional: Imprimir lo que SÍ se leyó para debug
                    // Console.WriteLine($"   Datos leídos: {System.Text.Json.JsonSerializer.Serialize(fila)}");
                    continue;
                }

                // ✅ Si pasa la validación, agregar la fila
                datos.Add(fila);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error procesando fila {rowIdx}: {ex.Message}");
                continue;
            }
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
            "Asignado",
            "GrupoAsignacion",
            "GrupoAsignación"
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

            string grupoAsignacionValor = d.TryGetValue("GrupoAsignacion", out var v1) ? v1 
                                        : d.TryGetValue("GrupoAsignación", out var v2) ? v2 
                                        : "";

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
                GrupoAsignacion = grupoAsignacionValor,
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

                if (string.IsNullOrWhiteSpace(i.FechaSolicitud))
                {
                    throw new Exception($"Fecha vacía o nula para ticket {i.CodTicket}");
                }

                bool parseoExitoso = DateTime.TryParseExact(
                    i.FechaSolicitud.Trim(),
                    new[] {
                        "yyyy-MM-dd HH:mm:ss",      // 2025-11-05 16:53:00
                        "yyyy-M-d HH:mm:ss",        // 2025-11-5 16:53:00
                        "d/M/yyyy HH:mm:ss",        // 5/11/2025 16:53:00
                        "d/M/yyyy  HH:mm:ss",       // Con doble espacio
                        "M/d/yy HH:mm",             // 10/13/25 18:04 (formato americano)
                        "MM/dd/yy HH:mm",           // 10/13/25 18:04 (con ceros)
                        "dd-MMM-yyyy",              // Formato anterior
                        "dd/MM/yyyy HH:mm:ss",      // Formato anterior
                        "M/d/yy",                   // 3/10/26 sin tiempo
                        "d/M/yy",                   // 10/3/26 sin tiempo
                        "MM/dd/yyyy",               // 03/10/2026 sin tiempo
                        "dd/MM/yyyy",               // 10/03/2026 sin tiempo
                        "M/d/yyyy",                 // 3/10/2026 sin tiempo
                        "d/M/yyyy"                  // 10/3/2026 sin tiempo
                    },
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces,
                    out fechaAsignacion
                );

                if (!parseoExitoso)
                {
                    throw new Exception($"No se pudo parsear la fecha '{i.FechaSolicitud}' para ticket {i.CodTicket}. Formatos válidos: yyyy-MM-dd HH:mm:ss, d/M/yyyy HH:mm:ss, M/d/yy HH:mm, M/d/yy, d/M/yy");
                }

                fechaAsignacion = DateTime.SpecifyKind(fechaAsignacion, DateTimeKind.Local);

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

                var frenteSubFrentesInsertList = new List<TicketFrenteSubFrenteInsertDto>();
                if (!string.IsNullOrWhiteSpace(i.GrupoAsignacion))
                {
                    var grupo = i.GrupoAsignacion.Trim();
                    var matchingSubFrente = _listaSubFrentes?.FirstOrDefault(sf =>
                        ParseValor1(sf.Valor1).Any(v => v.Equals(grupo, StringComparison.OrdinalIgnoreCase))
                    );

                    if (matchingSubFrente != null)
                    {
                        frenteSubFrentesInsertList.Add(new TicketFrenteSubFrenteInsertDto
                        {
                            IdFrente = matchingSubFrente.IdFrente,
                            IdSubFrente = matchingSubFrente.Id,
                            Cantidad = 1
                        });
                    }
                }

                // 🔹 Construir TicketInsertDto
                return new TicketInsertMasivoDto
                {
                    CodTicketInterno = LimpiarCodTicketInterno(i.CodTicket),
                    Titulo = i.Titulo,
                    FechaSolicitud = fechaAsignacion,
                    IdTipoTicket = _listaTipoTicket.FirstOrDefault(t => t.Codigo.Equals(AppConstants.TipoTicket.MesaDeAyuda, StringComparison.OrdinalIgnoreCase)).Id,
                    IdSubTipoTicket = LogicaObtenerSubTipoTicketPorCodigo(i.CodTicket.Trim()),
                    IdEstadoTicket = MapearEstado(i.EstadoTicket),
                    IdEmpresa = _empresaDto.Id,
                    IdUsuarioResponsableCliente = _empresaDto.IdUser,
                    IdPrioridad = MapearPrioridad(i.IdPrioridad),
                    Descripcion = i.Descripcion,
                    UsuarioCreacion = i.UsuarioCreacion,
                    IdGestor = (int)_empresaDto.IdGestor,
                    IdGestorConsultoria = 100,
                    EsCargaMasiva = true,
                    DatosCargaMasiva = System.Text.Json.JsonSerializer.Serialize(i, new JsonSerializerOptions { WriteIndented = true }),
                    GrupoAsignacion = i.GrupoAsignacion,
                    ConsultorAsignaciones = ticketConsultorInsertDto != null
                        ? new List<TicketConsultorAsignacionInsertDto> { ticketConsultorInsertDto }
                        : new List<TicketConsultorAsignacionInsertDto>(),
                    FrenteSubFrentes = frenteSubFrentesInsertList
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

    public async Task<List<Ticket>> CreateMasivoAsync(List<TicketInsertMasivoDto> insertDtos)
    {
        if (insertDtos == null || !insertDtos.Any())
            return new List<Ticket>();

        // 🔹 Obtener el código base del tipo de ticket (una sola vez)
        //string codigoTipoTicket = ObtenerCodigoTicketPorId()

        // 🔹 Obtener el último Id de ticket ya existente
        int ultimoId = (await _ticketRepository.GetAllAsync()).DefaultIfEmpty().Max(t => t?.Id ?? 0);

        // 🔹 Generar tickets con código incremental y mapear propiedades
        var tickets = insertDtos.Select((insertDto, index) =>
        {
            int nextId = ultimoId + index + 1;
            string fechaHora = DateTime.Now.ToString("yyyyMMdd");
            string codTicket = $"{ObtenerCodigoTicketPorId(insertDto.IdTipoTicket)}-{fechaHora}-{nextId}";

            var ticket = new Ticket
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
                UsuarioCreacion = "CargaMasivaExcel",
                IdGestor = insertDto.IdGestor,
                Activo = true,
                FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
                UrlArchivos = null,
                CodTicket = codTicket,
                EsCargaMasiva = true,
                IdGestorConsultoria = insertDto.IdGestorConsultoria,
                DatosCargaMasiva = insertDto.DatosCargaMasiva,
                // 🔹 Mapear asignaciones de consultores sin IdTicket aún
                ConsultorAsignaciones = insertDto.ConsultorAsignaciones?
                    .Select(a =>
                    {
                        var firstFsf = insertDto.FrenteSubFrentes?.FirstOrDefault();
                        return new TicketConsultorAsignacion
                        {
                            Id = 0, // evita duplicados al insertar
                            IdConsultor = a.IdConsultor,
                            IdTipoActividad = a.IdTipoActividad,
                            FechaAsignacion = DateTime.SpecifyKind(a.FechaAsignacion, DateTimeKind.Local),
                            FechaDesasignacion = DateTime.SpecifyKind(a.FechaDesasignacion, DateTimeKind.Local),
                            IdFrente = firstFsf?.IdFrente,
                            IdSubFrente = firstFsf?.IdSubFrente,
                            Activo = true
                        };
                    }).ToList() ?? new List<TicketConsultorAsignacion>()
            };

            ticket.FrenteSubFrentes = insertDto.FrenteSubFrentes?
                .Select(fsf =>
                {
                    string descCortada = insertDto.Descripcion ?? "";
                    if (descCortada.Length > 200)
                    {
                        descCortada = descCortada.Substring(0, 200);
                    }

                    return new TicketFrenteSubFrente
                    {
                        Id = 0,
                        IdFrente = fsf.IdFrente,
                        IdSubFrente = fsf.IdSubFrente,
                        Cantidad = fsf.Cantidad,
                        Descripcion = descCortada,
                        FechaInicio = DateTime.SpecifyKind(insertDto.FechaSolicitud, DateTimeKind.Local),
                        FechaFin = DateTime.SpecifyKind(insertDto.FechaSolicitud, DateTimeKind.Local),
                        FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
                        UsuarioCreacion = "CargaMasivaExcel",
                        Activo = true
                    };
                }).ToList() ?? new List<TicketFrenteSubFrente>();

            // Vincular las asignaciones con sus especializaciones (TicketFrenteSubFrente) en memoria antes de guardar
            foreach (var ca in ticket.ConsultorAsignaciones)
            {
                if (ca.IdSubFrente.HasValue)
                {
                    var matchingFsf = ticket.FrenteSubFrentes
                        .FirstOrDefault(fsf => fsf.IdSubFrente == ca.IdSubFrente.Value);
                    if (matchingFsf != null)
                    {
                        ca.TicketFrenteSubFrente = matchingFsf;
                    }
                }
            }

            return ticket;
        }).ToList();

        // 🔹 Guardar todos los tickets en batch
        var ticketsGuardados = await _ticketRepository.CreateRangeAsync(tickets);
        return ticketsGuardados;
    }

    private List<string> ParseValor1(string? valor1)
    {
        if (string.IsNullOrWhiteSpace(valor1))
            return new List<string>();

        var trimmed = valor1.Trim();
        if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
        {
            try
            {
                return JsonSerializer.Deserialize<List<string>>(trimmed) ?? new List<string>();
            }
            catch
            {
                // Fallback si la deserialización falla
            }
        }

        return new List<string> { trimmed };
    }


}
