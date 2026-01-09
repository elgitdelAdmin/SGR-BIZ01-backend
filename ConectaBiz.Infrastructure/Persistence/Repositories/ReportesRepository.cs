

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

        private async Task<IEnumerable<IDictionary<string, object>>> Query(string sql, object? p)
        {
            await using var cn = new NpgsqlConnection(_cs);
            var r = await cn.QueryAsync(sql, p);
            return r.Select(x => (IDictionary<string, object>)x);
        }
    }
}

