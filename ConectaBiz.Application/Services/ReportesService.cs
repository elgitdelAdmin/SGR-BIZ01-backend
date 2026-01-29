using ClosedXML.Excel;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using ConectaBiz.Domain.Constants;
using static ConectaBiz.Domain.Constants.AppConstants;

namespace ConectaBiz.Application.Services
{
    public class ReportesService : IReportesService
    {
        private readonly IReportesRepository _repo;
        private readonly IEmailService _email;
        private readonly IParametrosCatalogo _parametrosCatalogo;
        private readonly IParametroRepository _parametroRepo;

        // Variables para cachear datos de parámetros
        private IEnumerable<Parametro> _listaTipoTicket;
        private IEnumerable<Parametro> _listaSubTipoTicket;
        private IEnumerable<Parametro> _listaEstados;
        private IEnumerable<Parametro> _listaPrioridades;
        private IEnumerable<Parametro> _listaParametros;
        private IEnumerable<Parametro> _listaTipoActividad;
        private IEnumerable<Parametro> _listaReportes;

        private const string ExcelMime =
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public ReportesService(
            IReportesRepository repo,
            IEmailService email,
            IParametrosCatalogo parametrosCatalogo,
            IParametroRepository parametroRepo)
        {
            _repo = repo;
            _email = email;
            _parametrosCatalogo = parametrosCatalogo;
            _parametroRepo = parametroRepo;
        }

        private async Task InicializarDatosAsync()
        {
            await _parametrosCatalogo.EnsureLoadedAsync();

            var snap = _parametrosCatalogo.Current;

            _listaParametros = snap.ListaParametros;
            _listaTipoTicket = snap.ListaTipoTicket;
            _listaSubTipoTicket = snap.ListaSubTipoTicket;
            _listaEstados = snap.ListaEstados;
            _listaPrioridades = snap.ListaPrioridades;
            _listaTipoActividad = snap.ListaTipoActividad;
            _listaReportes = snap.ListaReportes;
        }

        public async Task Enviar3ExcelsAsync(DateTime fechaDesdeAutorizados,DateTime fechaDesdeNoCerrados,List<string> emails)
        {

            // 1️⃣ Obtener data
            var r1 = await _repo.GetAutorizadosGestorCuentaAsync(fechaDesdeAutorizados);
            var r2 = await _repo.GetNoCerradosAsync(fechaDesdeNoCerrados);
            var r3 = await _repo.GetDetalleTareasConsultorAsync();

            // 2️⃣ Generar Excel
            var e1 = CrearExcel(r1, "AUTORIZADOS");
            var e2 = CrearExcel(r2, "NO_CERRADOS");
            var e3 = CrearExcel(r3, "DETALLE_TAREAS");

            // 3️⃣ Enviar correo (EmailService EXISTENTE)
            await _email.EnviarCorreosConAdjuntosAsync(
                emails,
                "Reportes ConectaBiz",
                                $@"\
                Se adjuntan los siguientes reportes:

                • Autorizados (desde {fechaDesdeAutorizados:yyyy-MM-dd})
                • No Cerrados (desde {fechaDesdeNoCerrados:yyyy-MM-dd})
                • Detalle de Tareas
                ".Trim(),
                new[]
                {
                    ($"REP_AUTORIZADOS.xlsx", e1, ExcelMime),
                    ($"REP_NO_CERRADOS.xlsx", e2, ExcelMime),
                    ($"REP_DETALLE_TAREAS.xlsx", e3, ExcelMime)
                }
            );
        }

        public async Task<IEnumerable<IDictionary<string, object>>> ConsultarDetalleReporteAsync(FiltrosReporteRequest filtros)
        {
            await InicializarDatosAsync();
            // Lógica anterior de reporteSeleccionado removida/simplificada ya que se maneja en CallRepo
            
            return await CallRepo(filtros);
        }

        public async Task<byte[]> GenerarReporteExcelAsync(FiltrosReporteRequest filtros)
        {
            await InicializarDatosAsync();
            var data = await CallRepo(filtros);
            
            var configReporte = _listaReportes.FirstOrDefault(x => x.Id == filtros.IdTipoReporte || (filtros.CodigoReporte != null && x.Codigo == filtros.CodigoReporte));
            
            // Fallback DB si no está en cache (mismo patrón que CallRepo)
            if (configReporte == null)
                configReporte = await _parametroRepo.GetByIdAsync(filtros.IdTipoReporte);

            var sheetName = configReporte?.Nombre ?? "REPORTE";
            return CrearExcel(data, sheetName);
        }

        private async Task<IEnumerable<IDictionary<string, object>>> CallRepo(FiltrosReporteRequest f)
        {
            await InicializarDatosAsync();

            // 1. Buscar en CACHE (filtrado por Activo=true)
            var configReporte = _listaReportes.FirstOrDefault(x => x.Id == f.IdTipoReporte || (f.CodigoReporte != null && x.Codigo == f.CodigoReporte));

            // 2. Fallback DB DIRECTO (FindById no filtra por Activo, útil si el reporte está inactivo o cache desactualizado)
            if (configReporte == null)
            {
               configReporte = await _parametroRepo.GetByIdAsync(f.IdTipoReporte);
            }

            if (configReporte != null && !string.IsNullOrEmpty(configReporte.Valor1))
            {
                var sqlTemplate = configReporte.Valor1; // Ej: "SELECT func(:p_fecha_inicio, ...)"

                // Reemplazos de placeholders para Dapper (@Param)
                // Se asume que el template usa sintaxis tipo ":p_nombre"
                var sqlFinal = sqlTemplate
                    .Replace(":p_fecha_inicio", "CAST(@p_fecha_inicio AS date)")
                    .Replace(":p_fecha_fin", "CAST(@p_fecha_fin AS date)")
                    .Replace(":p_id_socio", "CAST(@p_id_socio AS integer)")
                    .Replace(":p_id_tickets", "CAST(@p_id_tickets AS integer[])")
                    .Replace(":p_id_tipos", "CAST(@p_id_tipos AS integer[])")
                    .Replace(":p_id_subtipos", "CAST(@p_id_subtipos AS integer[])")
                    .Replace(":p_id_estados", "CAST(@p_id_estados AS integer[])")
                    .Replace(":p_id_consultores", "CAST(@p_id_consultores AS integer[])")
                    .Replace(":p_id_empresas", "CAST(@p_id_empresas AS integer[])");

                var parameters = new
                {
                    p_fecha_inicio = f.FechaInicio,
                    p_fecha_fin = f.FechaFin,
                    p_id_socio = (f.IdSocio == null || f.IdSocio == 0) ? null : f.IdSocio,
                    p_id_tickets = (f.IdTickets != null && f.IdTickets.Any()) ? f.IdTickets.ToArray() : null,
                    p_id_tipos = (f.IdTiposTicket != null && f.IdTiposTicket.Any()) ? f.IdTiposTicket.ToArray() : null,
                    p_id_subtipos = (f.IdSubtiposTicket != null && f.IdSubtiposTicket.Any()) ? f.IdSubtiposTicket.ToArray() : null,
                    p_id_estados = (f.IdEstadosTicket != null && f.IdEstadosTicket.Any()) ? f.IdEstadosTicket.ToArray() : null,
                    p_id_consultores = (f.IdConsultores != null && f.IdConsultores.Any()) ? f.IdConsultores.ToArray() : null,
                    p_id_empresas = (f.IdEmpresas != null && f.IdEmpresas.Any()) ? f.IdEmpresas.ToArray() : null
                };

                return await _repo.EjecutarReporteDinamicoAsync(sqlFinal, parameters);
            }

            var idsDisponibles = string.Join(", ", _listaParametros.Select(x => x.Id));
            Console.WriteLine($"[DEBUG] Buscando Reporte ID: {f.IdTipoReporte}, Cod: {f.CodigoReporte}");
            Console.WriteLine($"[DEBUG] Total Parametros en memoria: {_listaParametros.Count()}");
            Console.WriteLine($"[DEBUG] IDs Disponibles: {idsDisponibles}");

            throw new Exception($"No se encontró la configuración para el reporte (ID: {f.IdTipoReporte}, Cod: {f.CodigoReporte}) en el catálogo de parámetros. Total: {_listaParametros.Count()}.");
        }

        private static byte[] CrearExcel(IEnumerable<IDictionary<string, object>> data, string sheet)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheet);

            var rows = data?.ToList() ?? new List<IDictionary<string, object>>();

            if (!rows.Any())
            {
                ws.Cell(1, 1).Value = "Sin registros";
            }
            else
            {
                var headers = rows.First().Keys.ToList();

                // Headers
                for (int c = 0; c < headers.Count; c++)
                    ws.Cell(1, c + 1).Value = headers[c];

                // Data
                int r = 2;
                foreach (var row in rows)
                {
                    for (int c = 0; c < headers.Count; c++)
                    {
                        var key = headers[c];
                        row.TryGetValue(key, out var val);

                        SetCellValue(ws.Cell(r, c + 1), val);
                    }
                    r++;
                }

                ws.RangeUsed().CreateTable();
                ws.SheetView.FreezeRows(1);
                ws.Columns().AdjustToContents();
            }

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        private static void SetCellValue(IXLCell cell, object? value)
        {
            if (value is null || value is DBNull)
            {
                cell.Clear();
                return;
            }

            // Soportar los tipos más comunes que vienen de PostgreSQL/Dapper
            switch (value)
            {
                case string s:
                    cell.Value = s;
                    return;

                case int i:
                    cell.Value = i;
                    return;

                case long l:
                    cell.Value = l;
                    return;

                case short sh:
                    cell.Value = sh;
                    return;

                case decimal d:
                    cell.Value = d;
                    return;

                case double db:
                    cell.Value = db;
                    return;

                case float f:
                    cell.Value = (double)f;
                    return;

                case bool b:
                    cell.Value = b;
                    return;

                case DateTime dt:
                    cell.Value = dt;
                    return;

                case DateOnly dateOnly:
                    cell.Value = dateOnly.ToDateTime(TimeOnly.MinValue);
                    return;

                case TimeOnly timeOnly:
                    cell.Value = timeOnly.ToTimeSpan();
                    return;

                case TimeSpan ts:
                    cell.Value = ts;
                    return;

                case Guid g:
                    cell.Value = g.ToString();
                    return;

                default:
                    // fallback seguro
                    cell.Value = value.ToString();
                    return;
            }
        }

    }
}
