using ConectaBiz.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.DTOs;

// ===== DTOs COMPLETOS (responses) =====
public class TicketDto
{
    public int Id { get; set; }
    public int? HorasTrabajadas { get; set; }
    public int? HorasPlanificadas { get; set; }
    public string CodTicket { get; set; } = string.Empty;
    public string CodTicketInterno { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public DateTime FechaSolicitud { get; set; }
    public int IdTipoTicket { get; set; }
    public int IdEstadoTicket { get; set; }
    public int IdEmpresa { get; set; }
    public int IdUsuarioResponsableCliente { get; set; }
    public int IdPrioridad { get; set; }
    public string? Descripcion { get; set; }
    public string? UrlArchivos { get; set; }
    public int? IdReqSgrCsti { get; set; }
    public string? CodReqSgrCsti { get; set; }
    public int? IdGestorConsultoria { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public string UsuarioCreacion { get; set; }
    public string? UsuarioActualizacion { get; set; }
    public bool EsCargaMasiva { get; set; }
    public EmpresaDto? Empresa { get; set; }
    public virtual List<TicketConsultorAsignacionDto> ConsultorAsignaciones { get; set; } = new List<TicketConsultorAsignacionDto>();
    public virtual List<TicketFrenteSubFrenteDto> FrenteSubFrentes { get; set; } = new List<TicketFrenteSubFrenteDto>();
    public virtual List<TicketHistorialEstadoDto> Historial { get; set; } = new List<TicketHistorialEstadoDto>();
}
public class TicketConsultorAsignacionDto
{
    public int? Id { get; set; } 
    public int IdTicket { get; set; }
    public int IdConsultor { get; set; }
    public int IdTipoActividad { get; set; }
    public DateTime FechaAsignacion { get; set; }
    public DateTime FechaDesasignacion { get; set; }
    public bool Activo { get; set; } = true;

    public List<DetalleTareasConsultorDto> DetalleTareasConsultor { get; set; } = new();
}

public class DetalleTareasConsultorDto
{
    public int Id { get; set; }
    public int IdTicketConsultorAsignacion { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal Horas { get; set; }
    public string Descripcion { get; set; }
    public bool Activo { get; set; }
}

public class DetalleTareasConsultorResponseDto
{
    public int Id { get; set; }
    public int IdTicketConsultorAsignacion { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal Horas { get; set; }
    public string Descripcion { get; set; }
    public bool Activo { get; set; }
}


public class TicketFrenteSubFrenteDto
{
    public int Id { get; set; } // Null para nuevos registros
    public int IdTicket { get; set; } // Se asigna automáticamente
    public int IdFrente { get; set; }
    public int IdSubFrente { get; set; }
    public int Cantidad { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string UsuarioCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? UsuarioModificacion { get; set; }
    public bool Activo { get; set; } = true;
}

public class TicketHistorialEstadoDto
{
    public int Id { get; set; } // Null para nuevos registros
    public int IdTicket { get; set; } // Se asigna automáticamente
    public int IdEstadoAnterior { get; set; }
    public int IdEstadoNuevo { get; set; }
    public int IdConsultorAnterior { get; set; }
    public int IdConsultorNuevo { get; set; }
    public DateTime FechaCambio { get; set; }
}

// ===== TICKET DTOs =====

public class TicketZipFileDto
{
    public int Orden { get; set; }
    public string Url { get; set; }
    public DateTime FechaInsert{ get; set; }
}

public class TicketInsertDto
{
    public string CodTicketInterno { get; set; }
    public string Titulo { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public int IdTipoTicket { get; set; }
    public int IdEstadoTicket { get; set; }
    public int IdEmpresa { get; set; }
    public int? IdUsuarioResponsableCliente { get; set; }
    public int IdPrioridad { get; set; }
    public string Descripcion { get; set; }
    public string? UrlArchivos { get; set; }
    public string UsuarioCreacion { get; set; }
    public string? CodReqSgrCsti {get;set; }
    public int? IdReqSgrCsti { get; set; }
    public int? IdGestorConsultoria { get; set; }

    // Nuevo campo para el .zip
    public IFormFile? ZipFile { get; set; }
    public bool? EsCargaMasiva { get; set; }

    // Colecciones relacionadas que siempre vienen en el request
    //public string consultorAsignaciones { get; set; }    
    //public List<TicketConsultorAsignacionInsertDto> ConsultorAsignaciones { get; set; } = new List<TicketConsultorAsignacionInsertDto>();
    //public string frenteSubFrentes { get; set; }
    //public List<TicketFrenteSubFrenteInsertDto> FrenteSubFrentes { get; set; } = new List<TicketFrenteSubFrenteInsertDto>();
}

public class TicketUpdateDto
{
    public string CodTicketInterno { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public DateTime FechaSolicitud { get; set; }
    public int IdTipoTicket { get; set; }
    public int IdEstadoTicket { get; set; }
    public int IdEmpresa { get; set; }
    public int IdUsuarioResponsableCliente { get; set; }
    public int IdPrioridad { get; set; }
    public string Descripcion { get; set; }
    public string? UrlArchivos { get; set; }
    public int? IdGestorConsultoria { get; set; }
    // Nuevo campo para el .zip
    public IFormFile? ZipFile { get; set; }
    public string UsuarioActualizacion { get; set; }
    public string consultorAsignaciones { get; set; }
    public List<TicketConsultorAsignacionUpdateDto> ConsultorAsignaciones { get; set; } = new List<TicketConsultorAsignacionUpdateDto>();
    public string frenteSubFrentes { get; set; }
    public List<TicketFrenteSubFrenteUpdateDto> FrenteSubFrentes { get; set; } = new List<TicketFrenteSubFrenteUpdateDto>();
}

// ===== TICKET FRENTE SUB FRENTE DTOs =====
public class TicketFrenteSubFrenteInsertDto
{
    public int IdFrente { get; set; }
    public int IdSubFrente { get; set; }
    public int Cantidad { get; set; }
}

public class TicketFrenteSubFrenteUpdateDto
{
    public int Id { get; set; }
    public int IdFrente { get; set; }
    public int IdSubFrente { get; set; }
    public int Cantidad { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public DateTime FechaModificacion { get; set; }
    public bool Activo { get; set; }
    public string UsuarioActualizacion { get; set; }
}

// ===== TICKET CONSULTOR ASIGNACIONES DTOs =====
public class TicketConsultorAsignacionInsertDto
{
    public int IdConsultor { get; set; }
    public int IdTipoActividad { get; set; }
    public DateTime FechaAsignacion { get; set; }
    public DateTime FechaDesasignacion { get; set; }
}
public class TicketConsultorAsignacionUpdateDto
{
    public int Id { get; set; }
    public int IdConsultor { get; set; }
    public int IdTipoActividad { get; set; }
    public DateTime FechaAsignacion { get; set; }
    public DateTime FechaDesasignacion { get; set; }
    public bool Activo { get; set; } = true;
    public List<DetalleTareasConsultorUpdateDto> DetalleTareasConsultor { get; set; } = new();
}
public class DetalleTareasConsultorUpdateDto
{
    public int Id { get; set; }
    public int IdTicketConsultorAsignacion { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal Horas { get; set; }
    public string Descripcion { get; set; }
    public bool Activo { get; set; } = true;
}