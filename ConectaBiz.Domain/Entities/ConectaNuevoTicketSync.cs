namespace ConectaBiz.Domain.Entities;

public class ConectaNuevoTicketSync
{
    public string CodRequerimiento { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string? Detalle { get; set; }
    public DateTime FechaRegistro { get; set; }
}
