using ClosedXML.Excel;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Interfaces;

namespace ConectaBiz.Application.Services
{
    public class ReportesService : IReportesService
    {
        private readonly IReportesRepository _repo;
        private readonly IEmailService _email;

        private const string ExcelMime =
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public ReportesService(
            IReportesRepository repo,
            IEmailService email)
        {
            _repo = repo;
            _email = email;
        }

        public async Task Enviar3ExcelsAsync(
            DateTime fechaDesdeAutorizados,
            DateTime fechaDesdeNoCerrados,
            List<string> emails)
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
                                $@"
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
