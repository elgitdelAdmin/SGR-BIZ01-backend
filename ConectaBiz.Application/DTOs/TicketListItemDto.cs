namespace ConectaBiz.Application.DTOs;

/// <summary>
/// DTO ligero para listados paginados de tickets (sin relaciones pesadas).
/// </summary>
public class TicketListItemDto
{
    public int Id { get; set; }
    public string CodTicket { get; set; } = string.Empty;
    public string CodTicketInterno { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public DateTime FechaSolicitud { get; set; }
    public int IdTipoTicket { get; set; }
    public string? TipoTicketNombre { get; set; }
    public int? IdSubTipoTicket { get; set; }
    public string? SubTipoTicketNombre { get; set; }
    public string? TipoSubtipoNombre { get; set; }
    public int IdEstadoTicket { get; set; }
    public string EstadoNombre { get; set; } = string.Empty;
    public int IdPrioridad { get; set; }
    public string PrioridadNombre { get; set; } = string.Empty;
    public int IdEmpresa { get; set; }
    public string? EmpresaRazonSocial { get; set; }
    public string? NombreGestor { get; set; }
    /// <summary>
    /// Nombres completos de los consultores asignados al ticket (separados por coma).
    /// </summary>
    public string? NombreConsultores { get; set; }
    public decimal HorasTrabajadas { get; set; }
    public decimal HorasPlanificadas { get; set; }
    public DateTime? FechaCreacion { get; set; }
}
