

using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Interfaces;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ConectaBiz.Infrastructure.Persistence.Repositories
{
    public class ReportesRepository : IReportesRepository
    {
        private readonly string _cs;

        public ReportesRepository(IConfiguration cfg)
        {
            _cs = cfg.GetConnectionString("DefaultConnection");
        }

        public Task<IEnumerable<IDictionary<string, object>>> GetAutorizadosGestorCuentaAsync(DateTime fecha)
            => Query(@"SELECT * FROM conectabiz.""REP_AUTORIZADOS_GESTORCUENTA""(@f::date);",
              new { f = fecha });

        public Task<IEnumerable<IDictionary<string, object>>> GetNoCerradosAsync(DateTime fecha)
            => Query(@"SELECT * FROM conectabiz.""REP_NO_CERRADOS""(@f::date);",
                     new { f = fecha });

        public Task<IEnumerable<IDictionary<string, object>>> GetDetalleTareasConsultorAsync()
            => Query(@"SELECT * FROM conectabiz.""REP_DETALLE_TAREAS_CONSULTOR""();", null);

        public async Task<IEnumerable<IDictionary<string, object>>> ObtenerDatosReporteAsync(
            int? idTipoReporte,
            string? codigoReporte,
            List<int>? idEmpresas,
            List<int>? idTickets,
            List<int>? idTiposTicket,
            List<int>? idSubtiposTicket,
            List<int>? idEstadosTicket,
            List<int>? idConsultores,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? idUser,
            string? codRol)
        {
            // Llamar a un stored procedure o función que maneje todos los filtros
            var sql = @"SELECT * FROM conectabiz.""ObtenerReporteDinamico""(
                @pIdTipoReporte,
                @pCodigoReporte,
                @pIdEmpresas,
                @pIdTickets,
                @pIdTiposTicket,
                @pIdSubtiposTicket,
                @pIdEstadosTicket,
                @pIdConsultores,
                @pFechaInicio,
                @pFechaFin,
                @pIdUser,
                @pCodRol
            );";

            var parameters = new
            {
                pIdTipoReporte = idTipoReporte,
                pCodigoReporte = codigoReporte,
                pIdEmpresas = idEmpresas?.ToArray() ?? Array.Empty<int>(),
                pIdTickets = idTickets?.ToArray() ?? Array.Empty<int>(),
                pIdTiposTicket = idTiposTicket?.ToArray() ?? Array.Empty<int>(),
                pIdSubtiposTicket = idSubtiposTicket?.ToArray() ?? Array.Empty<int>(),
                pIdEstadosTicket = idEstadosTicket?.ToArray() ?? Array.Empty<int>(),
                pIdConsultores = idConsultores?.ToArray() ?? Array.Empty<int>(),
                pFechaInicio = fechaInicio,
                pFechaFin = fechaFin,
                pIdUser = idUser,
                pCodRol = codRol
            };

            return await Query(sql, parameters);
        }

        public async Task<IEnumerable<IDictionary<string, object>>> EjecutarReporteDinamicoAsync(string sql, object parameters)
        {
            try
            {
                return await QueryReporteDinamico(sql, parameters);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<IEnumerable<IDictionary<string, object>>> Query(string sql, object? p)
        {
            await using var cn = new NpgsqlConnection(_cs);
            var r = await cn.QueryAsync(sql, p);
            return r.Select(x => (IDictionary<string, object>)x);
        }

        private async Task<IEnumerable<IDictionary<string, object>>> QueryReporteDinamico(string sql, object? p)
        {
            await using var cn = new NpgsqlConnection(_cs);
            var r = await cn.QueryAsync(sql, p);
            
            var result = new List<IDictionary<string, object>>();
            
            Console.WriteLine($"[DEBUG] QueryReporteDinamico - Original SQL: {sql}");
            Console.WriteLine($"[DEBUG] QueryReporteDinamico - Rows returned: {((IEnumerable<dynamic>)r).Count()}");
            
            if (!r.Any())
                return result;
            
            var firstRow = r.First();
            var rowDict = (IDictionary<string, object>)firstRow;
            
            Console.WriteLine($"[DEBUG] First row has {rowDict.Count} columns");
            foreach (var kvp in rowDict)
            {
                Console.WriteLine($"[DEBUG]   Column: {kvp.Key}, Type: {kvp.Value?.GetType()?.FullName ?? "NULL"}");
            }
            
            // Si la función devuelve un solo campo y es un array (caso System.Object[])
            if (rowDict.Count == 1)
            {
                var firstKey = rowDict.Keys.First();
                var firstValue = rowDict.Values.First();
                
                // Si el valor es un array de objetos (composite type de PostgreSQL)
                if (firstValue is object[] arrayValues)
                {
                    Console.WriteLine($"[DEBUG] Detected System.Object[] - attempting to rewrite SQL");
                    
                    // NUEVA ESTRATEGIA: usar alias de tabla para forzar expansión
                    // De: SELECT * FROM schema."function"(params)
                    // A:  SELECT t.* FROM schema."function"(params) AS t
                    var sqlRewritten = RewriteSqlWithTableAlias(sql);
                    
                    Console.WriteLine($"[DEBUG] Rewritten SQL: {sqlRewritten}");
                    
                    // Re-ejecutar con el SQL reescrito
                    var r2 = await cn.QueryAsync(sqlRewritten, p);
                    
                    Console.WriteLine($"[DEBUG] Rewritten query returned {((IEnumerable<dynamic>)r2).Count()} rows");
                    
                    if (r2.Any())
                    {
                        var firstRow2 = r2.First();
                        var rowDict2 = (IDictionary<string, object>)firstRow2;
                        Console.WriteLine($"[DEBUG] Rewritten first row has {rowDict2.Count} columns");
                        foreach (var kvp in rowDict2.Take(3))
                        {
                            Console.WriteLine($"[DEBUG]   Column: {kvp.Key}, Type: {kvp.Value?.GetType()?.FullName ?? "NULL"}");
                        }
                    }
                    
                    var expandedResult = new List<IDictionary<string, object>>();
                    foreach (var row2 in r2)
                    {
                        var dict2 = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                        foreach (var kvp in (IDictionary<string, object>)row2)
                        {
                            dict2[kvp.Key] = kvp.Value;
                        }
                        expandedResult.Add(dict2);
                    }
                    
                    Console.WriteLine($"[DEBUG] Returning {expandedResult.Count} expanded rows");
                    return expandedResult;
                }
            }
            
            // Caso normal: múltiples columnas, procesamiento directo
            foreach (var row in r)
            {
                var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (var kvp in (IDictionary<string, object>)row)
                {
                    dict[kvp.Key] = kvp.Value;
                }
                result.Add(dict);
            }
            
            return result;
        }

        private string RewriteSqlWithTableAlias(string sql)
        {
            // Detectar si es SELECT function() o SELECT * FROM function()
            
            // Caso 1: SELECT * FROM schema.function(params) -> SELECT t.* FROM schema.function(params) AS t
            if (sql.Contains("FROM", StringComparison.OrdinalIgnoreCase))
            {
                var pattern = @"SELECT\s+\*\s+FROM\s+([\w\.""]+\([^\)]*\))";
                var replacement = "SELECT t.* FROM $1 AS t";
                
                return System.Text.RegularExpressions.Regex.Replace(
                    sql, 
                    pattern, 
                    replacement, 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
            }
            
            // Caso 2: SELECT schema.function(params) -> SELECT (schema.function(params)).*
            // El problema es que tenemos paréntesis anidados (CAST), así que en lugar de regex,
            // simplemente envolvemos todo el SELECT en paréntesis
            
            // Remover el SELECT inicial
            var sqlWithoutSelect = sql.Trim();
            if (sqlWithoutSelect.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase))
            {
                sqlWithoutSelect = sqlWithoutSelect.Substring(7).Trim(); // Quitar "SELECT "
            }
            
            // Ahora tenemos: conectabiz.fn_cargabilidad_por_tickets(CAST(@p_fecha_inicio AS date), ...)
            // Lo envolvemos: SELECT (conectabiz.fn_cargabilidad_por_tickets(...)).*
            var rewritten = $"SELECT ({sqlWithoutSelect}).*";
            
            Console.WriteLine($"[DEBUG] Direct function call detected, rewrote to: {rewritten}");
            
            return rewritten;
        }

        private string RewriteSqlToExpandCompositeType(string sql)
        {
            // Patrón original por si acaso se necesita
            var pattern = @"SELECT\s+\*\s+FROM\s+([\w\.""]+\([^\)]*\))";
            var replacement = "SELECT ($1).*";
            
            return System.Text.RegularExpressions.Regex.Replace(
                sql, 
                pattern, 
                replacement, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );
        }
    }
}

