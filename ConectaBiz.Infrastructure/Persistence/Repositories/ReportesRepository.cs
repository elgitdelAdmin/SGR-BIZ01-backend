

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
                return await Query(sql, parameters);

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
    }
}

