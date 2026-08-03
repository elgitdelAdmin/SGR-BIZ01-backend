using ConectaBiz.Application.Interfaces;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Text.Json;

namespace ConectaBiz.Infrastructure.Services
{
    public class ExcelService : IExcelService
    {
        public byte[] GenerateExcel<T>(IEnumerable<T> data, string sheetName = "Sheet1")
        {
            throw new NotImplementedException("Method not implemented yet.");
        }

        public List<T> ReadExcel<T>(Stream stream, string tipoCarga)
        {
            throw new NotImplementedException("Method not implemented yet.");
        }

        public List<Dictionary<string, string>> LeerFilasComoDict(Stream stream, HashSet<string> columnasObligatorias)
        {
            var datos = new List<Dictionary<string, string>>();
            
            XSSFWorkbook workbook = new XSSFWorkbook(stream);
            var sheet = workbook.GetSheetAt(0);
            if (sheet == null) return datos;

            var headerRow = sheet.GetRow(0);
            if (headerRow == null) return datos;

            // Obtener TODAS las columnas con encabezado
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

            Console.WriteLine($"🔍 Columnas detectadas en Excel: {string.Join(", ", columnas.Select(c => c.Header))}");

            var formatter = new DataFormatter();

            for (int rowIdx = 1; rowIdx <= sheet.LastRowNum; rowIdx++)
            {
                var row = sheet.GetRow(rowIdx);
                if (row == null) continue;

                // Verificar que la fila tenga contenido en al menos una celda
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

                var fila = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                try
                {
                    // Procesar todas las columnas
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

                    // VALIDAR que las columnas obligatorias tengan valor
                    bool filaValida = true;
                    var columnasVacias = new List<string>();

                    foreach (var columnaObligatoria in columnasObligatorias)
                    {
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
                        Console.WriteLine($"⚠️ Fila {rowIdx} omitida. Faltan: {string.Join(", ", columnasVacias)}");
                        continue;
                    }

                    datos.Add(fila);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error procesando fila {rowIdx}: {ex.Message}");
                    continue;
                }
            }

            return datos;
        }
    }
}
