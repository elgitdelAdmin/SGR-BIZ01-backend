using ConectaBiz.Domain.Entities;

namespace ConectaBiz.Domain.Interfaces;

public interface IConectaNuevoTicketRepository
{
    /// <summary>Inserta en Gineeri.BizPartner.Conecta.Datos. No hace nada si no hay cadena configurada.</summary>
    Task InsertarTicketAsync(ConectaNuevoTicketSync payload, CancellationToken cancellationToken = default);
}
