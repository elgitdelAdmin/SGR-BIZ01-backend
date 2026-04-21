using ConectaBiz.Domain.Constants;
using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace ConectaBiz.Infrastructure.Persistence.Repositories;

public class ConectaNuevoTicketRepository : IConectaNuevoTicketRepository
{
    /// <summary>
    /// Inserción explícita por nombre de columna (el INSERT posicional falla si la tabla tiene más columnas que valores).
    /// Orden alineado al esquema BizPartner Conecta (Tickets).
    /// </summary>
    private const string SqlInsert = """
        INSERT INTO dbo.Tickets (
            [Id],
            [Bloqueado], [Relacionado], [Eliminado], [Estado],
            [Título], [Resumen], [Detalle], [Adjuntos], [OrigenDeTicket],
            [EjecutivoComercial], [Oportunidad], [EjecutivoDeCuenta],
            [Contrato], [Periodo],
            [FechaDeRegistro], [Código], [Nombre], [Descripción]
        ) VALUES (
            NEWID(),
            0, 0, 0, 0,
            @titulo, N'', N'', N'', 2,
            N'-1', N'-1', N'-1',
            NULL, NULL,
            @fecharegistro, @codrequerimiento, @titulo, @detalle
        )
        """;

    public async Task InsertarTicketAsync(ConectaNuevoTicketSync payload, CancellationToken cancellationToken = default)
    {
        var cs = Conexiones.ConnectionConectaNuevo;
        if (string.IsNullOrWhiteSpace(cs))
            return;

        await using var conn = new SqlConnection(cs);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(SqlInsert, conn);
        cmd.Parameters.AddWithValue("@codrequerimiento", payload.CodRequerimiento);
        cmd.Parameters.AddWithValue("@titulo", payload.Titulo);
        cmd.Parameters.AddWithValue("@fecharegistro", payload.FechaRegistro);
        cmd.Parameters.AddWithValue("@detalle", (object?)payload.Detalle ?? DBNull.Value);

        try
        {
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            // Duplicado en destino; idempotente
        }
    }
}
