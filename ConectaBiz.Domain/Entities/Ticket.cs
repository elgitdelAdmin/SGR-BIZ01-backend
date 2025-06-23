using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Entities;

public class Ticket
{
    public int Id { get; set; }
    public string CodTicket { get; set; }
    public string CodTicketInterno { get; set; }
    public string Titulo { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public int IdTipoTicket { get; set; }
    public int IdEstadoTicket { get; set; }
    public int IdEmpresa { get; set; }
    public int IdUsuarioResponsableCliente { get; set; }
    public int IdPais { get; set; }
    public int IdPrioridad { get; set; }
    public string? Descripcion { get; set; }
    public string? UrlArchivos { get; set; }
    public int? IdReqSgrCsti { get; set; }
    public string? CodReqSgrCsti { get; set; }
    public bool Activo { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public string? UsuarioCreacion { get; set; }
    public string? UsuarioActualizacion { get; set; }
    public int IdGestorAsignado { get; set; }    
    public virtual ICollection<TicketConsultorAsignacion> ConsultorAsignaciones { get; set; } = new List<TicketConsultorAsignacion>();
    public virtual ICollection<TicketFrenteSubFrente> FrenteSubFrentes { get; set; } = new List<TicketFrenteSubFrente>();
    public virtual ICollection<TicketHistorialEstado> TicketHistorialEstado { get; set; } = new List<TicketHistorialEstado>();
}

public class TicketConsultorAsignacion
{
    public int Id { get; set; }
    public int IdTicket { get; set; }
    public int IdConsultor { get; set; }
    public int IdTipoActividad { get; set; }
    public DateTime FechaAsignacion { get; set; }
    public DateTime FechaDesasignacion { get; set; }
    public bool Activo { get; set; } = true;
    public virtual Ticket Ticket { get; set; } = null!;
}

public class TicketFrenteSubFrente
{
    public int Id { get; set; }
    public int IdTicket { get; set; }
    public int IdFrente { get; set; }
    public int IdSubFrente { get; set; }
    public int Cantidad { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string UsuarioCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? UsuarioModificacion { get; set; }
    public bool Activo { get; set; } = true;
    public virtual Ticket Ticket { get; set; } = null!;
}

public class TicketHistorialEstado
{
    public int Id { get; set; }
    public int IdTicket { get; set; }
    public int? IdEstadoAnterior { get; set; }
    public int? IdEstadoNuevo { get; set; }
    public DateTime FechaCambio { get; set; }
    public string? UsuarioCambio { get; set; }
    public virtual Ticket Ticket { get; set; } = null!;
}