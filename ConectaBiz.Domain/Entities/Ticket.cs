using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace ConectaBiz.Domain.Entities;

public class Ticket
{
    public int Id { get; set; }
    public string CodTicket { get; set; }
    public string CodTicketInterno { get; set; }
    public string Titulo { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public int IdTipoTicket { get; set; }
    public int? IdSubTipoTicket { get; set; }
    public int IdEstadoTicket { get; private set; }
    public int IdEmpresa { get; set; }
    public int? IdGestorConsultoria { get; set; }    
    public int IdUsuarioResponsableCliente { get; set; }
    public int IdPrioridad { get; set; }
    public string? Descripcion { get; set; }
    public string? UrlArchivos { get; set; }
    public string? Repositorios { get; set; }
    public int? IdReqSgrCsti { get; set; }
    public string? CodReqSgrCsti { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public string? UsuarioCreacion { get; set; }
    public string? UsuarioActualizacion { get; set; }
    public bool EsCargaMasiva { get; set; } = false;
    public string? DatosCargaMasiva { get; set; }

    [ForeignKey(nameof(IdEmpresa))]
    public virtual Empresa Empresa { get; set; }
    public virtual ICollection<TicketConsultorAsignacion> ConsultorAsignaciones { get; set; } = new List<TicketConsultorAsignacion>();
    public virtual ICollection<TicketFrenteSubFrente> FrenteSubFrentes { get; set; } = new List<TicketFrenteSubFrente>();
    public virtual ICollection<TicketHistorialEstado> TicketHistorialEstado { get; set; } = new List<TicketHistorialEstado>();

    public void InicializarEstado(int idEstadoInicial, string usuario)
    {
        IdEstadoTicket = idEstadoInicial;
        TicketHistorialEstado.Add(new TicketHistorialEstado
        {
            IdEstadoAnterior = idEstadoInicial,
            IdEstadoNuevo = idEstadoInicial,
            FechaCambio = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
            UsuarioCambio = usuario
        });
    }

    public void CambiarEstado(int idNuevoEstado, string usuario)
    {
        if (IdEstadoTicket == idNuevoEstado) return;

        int estadoAnterior = IdEstadoTicket;
        IdEstadoTicket = idNuevoEstado;

        TicketHistorialEstado.Add(new TicketHistorialEstado
        {
            IdEstadoAnterior = estadoAnterior,
            IdEstadoNuevo = idNuevoEstado,
            FechaCambio = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
            UsuarioCambio = usuario ?? "SYSTEM"
        });
    }

    public void EvaluarTransicionesAutomaticas(
        bool tieneNuevasAsignaciones,
        bool huboCambiosFrentes,
        int idEstadoAtendido, 
        int idEstadoPendienteAsig, 
        int idEstadoAsignado, 
        int idEstadoPendienteAtencion,
        string usuario)
    {
        // El Dominio evalúa su propio estado para determinar si existe planificación activa
        bool tienePlanificacion = this.FrenteSubFrentes.Any(f => 
            f.Activo && 
            f.DetallePlanificacionConsultor != null && 
            f.DetallePlanificacionConsultor.Any(p => p.Activo)
        );

        if (IdEstadoTicket == idEstadoPendienteAtencion && huboCambiosFrentes)
        {
            CambiarEstado(idEstadoAtendido, usuario);
        }
        else if (IdEstadoTicket == idEstadoAtendido && tienePlanificacion)
        {
            CambiarEstado(idEstadoPendienteAsig, usuario);
        }
        else if (IdEstadoTicket == idEstadoPendienteAsig && tieneNuevasAsignaciones)
        {
            CambiarEstado(idEstadoAsignado, usuario);
        }
    }

    public void ActualizarRepositorios(string? repositoriosNuevosJson)
    {
        if (string.IsNullOrWhiteSpace(repositoriosNuevosJson))
            return;

        var existentes = string.IsNullOrWhiteSpace(Repositorios)
            ? new List<RepositorioLink>()
            : JsonSerializer.Deserialize<List<RepositorioLink>>(Repositorios) ?? new List<RepositorioLink>();

        var incoming = JsonSerializer.Deserialize<List<JsonElement>>(repositoriosNuevosJson) ?? new List<JsonElement>();
        var ahora = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
        int ultimoOrden = existentes.Any() ? existentes.Max(x => x.Orden) : 0;
        var resultado = new List<RepositorioLink>();

        foreach (var item in incoming)
        {
            var url = item.TryGetProperty("Url", out var urlProp) ? urlProp.GetString() : 
                      item.TryGetProperty("Link", out var linkProp) ? linkProp.GetString() : null;

            url = (url ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(url)) continue;

            var existente = existentes.FirstOrDefault(e => string.Equals(e.Url?.Trim(), url, StringComparison.OrdinalIgnoreCase));

            if (existente != null)
            {
                resultado.Add(new RepositorioLink { Orden = existente.Orden, Url = existente.Url, FechaInsert = existente.FechaInsert });
            }
            else
            {
                ultimoOrden++;
                resultado.Add(new RepositorioLink { Orden = ultimoOrden, Url = url, FechaInsert = ahora });
            }
        }
        Repositorios = JsonSerializer.Serialize(resultado);
    }

    private class RepositorioLink
    {
        public int Orden { get; set; }
        public string? Url { get; set; }
        public DateTime? FechaInsert { get; set; }
    }
    public (int Agregados, int Modificados) ActualizarFrentes(IEnumerable<TicketFrenteSubFrente> frentesNuevos, string usuarioActualizacion)
    {
        int agregados = 0;
        int modificados = 0;

        if (frentesNuevos == null) return (0, 0);

        foreach (var frente in frentesNuevos)
        {
            frente.FechaInicio = DateTime.SpecifyKind(frente.FechaInicio, DateTimeKind.Local);
            frente.FechaFin = DateTime.SpecifyKind(frente.FechaFin, DateTimeKind.Local);

            if (frente.DetallePlanificacionConsultor != null)
            {
                foreach (var plan in frente.DetallePlanificacionConsultor)
                {
                    plan.FechaInicio = DateTime.SpecifyKind(plan.FechaInicio, DateTimeKind.Local);
                    plan.FechaFin = DateTime.SpecifyKind(plan.FechaFin, DateTimeKind.Local);
                }
            }

            if (frente.Id == 0)
            {
                frente.IdTicket = this.Id;
                frente.UsuarioCreacion = usuarioActualizacion;
                frente.FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                frente.FechaModificacion = null;
                this.FrenteSubFrentes.Add(frente);
                agregados++;
            }
            else
            {
                var existente = this.FrenteSubFrentes.FirstOrDefault(f => f.Id == frente.Id);
                if (existente != null)
                {
                    existente.IdFrente = frente.IdFrente;
                    existente.IdSubFrente = frente.IdSubFrente;
                    existente.Cantidad = frente.Cantidad;
                    existente.Descripcion = frente.Descripcion;
                    existente.FechaInicio = frente.FechaInicio;
                    existente.FechaFin = frente.FechaFin;
                    existente.Activo = frente.Activo;

                    existente.UsuarioModificacion = usuarioActualizacion;
                    existente.FechaModificacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

                    if (frente.Activo && frente.DetallePlanificacionConsultor != null)
                    {
                        foreach (var planNuevo in frente.DetallePlanificacionConsultor)
                        {
                            if (planNuevo.Id == 0)
                            {
                                existente.DetallePlanificacionConsultor.Add(planNuevo);
                            }
                            else
                            {
                                var planExistente = existente.DetallePlanificacionConsultor.FirstOrDefault(p => p.Id == planNuevo.Id);
                                if (planExistente != null)
                                {
                                    planExistente.IdTipoActividad = planNuevo.IdTipoActividad;
                                    planExistente.FechaInicio = planNuevo.FechaInicio;
                                    planExistente.FechaFin = planNuevo.FechaFin;
                                    planExistente.Horas = planNuevo.Horas;
                                    planExistente.Descripcion = planNuevo.Descripcion;
                                    planExistente.Activo = planNuevo.Activo;
                                    if (planNuevo.IdTicketConsultorAsignacion > 0)
                                    {
                                        planExistente.IdTicketConsultorAsignacion = planNuevo.IdTicketConsultorAsignacion;
                                    }
                                }
                            }
                        }
                    }
                    modificados++;
                }
            }
        }
        return (agregados, modificados);
    }
    public HashSet<int> ActualizarAsignaciones(IEnumerable<TicketConsultorAsignacion> asignacionesNuevas)
    {
        var nuevosIdsConsultores = new HashSet<int>();
        if (asignacionesNuevas == null) return nuevosIdsConsultores;

        // 1. Crear diccionarios de frentes activos para asociarlos a las asignaciones
        var frentesActivos = this.FrenteSubFrentes.Where(f => f.Activo).ToList();
        var frenteRefLookup = frentesActivos.ToLookup(f => f.IdSubFrente);
        var frenteRefByIdMap = frentesActivos.Where(f => f.Id > 0).ToDictionary(f => f.Id, f => f);
        var frentesUsados = new HashSet<TicketFrenteSubFrente>();

        foreach (var asignacion in asignacionesNuevas)
        {
            // Normalizar fechas a la zona horaria local
            asignacion.FechaAsignacion = DateTime.SpecifyKind(asignacion.FechaAsignacion, DateTimeKind.Local);
            asignacion.FechaDesasignacion = DateTime.SpecifyKind(asignacion.FechaDesasignacion, DateTimeKind.Local);
            
            if (asignacion.DetalleTareasConsultor != null)
            {
                foreach (var tarea in asignacion.DetalleTareasConsultor)
                {
                    tarea.FechaInicio = DateTime.SpecifyKind(tarea.FechaInicio, DateTimeKind.Local);
                    tarea.FechaFin = DateTime.SpecifyKind(tarea.FechaFin, DateTimeKind.Local);
                }
            }
            
            bool esPlaceholder = asignacion.IdConsultor == null || asignacion.IdConsultor == 0;

            // Encontrar el Frente/SubFrente asociado
            TicketFrenteSubFrente frenteAsociado = null;
            if (asignacion.IdTicketFrenteSubFrente.HasValue && asignacion.IdTicketFrenteSubFrente.Value > 0)
            {
                frenteRefByIdMap.TryGetValue(asignacion.IdTicketFrenteSubFrente.Value, out frenteAsociado);
            }
            
            if (frenteAsociado == null && asignacion.IdSubFrente.HasValue)
            {
                var candidatos = frenteRefLookup[asignacion.IdSubFrente.Value];
                frenteAsociado = candidatos.FirstOrDefault(c => !frentesUsados.Contains(c));
                if (frenteAsociado == null && candidatos.Any())
                {
                    frenteAsociado = candidatos.First();
                }
            }

            if (frenteAsociado != null) frentesUsados.Add(frenteAsociado);

            if (asignacion.Id == 0)
            {
                if (!esPlaceholder)
                {
                    asignacion.IdTicket = this.Id;
                    if (frenteAsociado != null)
                    {
                        if (frenteAsociado.Id > 0) asignacion.IdTicketFrenteSubFrente = frenteAsociado.Id;
                        else asignacion.TicketFrenteSubFrente = frenteAsociado; // Referencia en memoria para EF
                    }
                    this.ConsultorAsignaciones.Add(asignacion);

                    if (asignacion.IdConsultor.HasValue && asignacion.IdConsultor.Value > 0)
                    {
                        nuevosIdsConsultores.Add(asignacion.IdConsultor.Value);
                    }
                }
            }
            else
            {
                if (!esPlaceholder)
                {
                    var existente = this.ConsultorAsignaciones.FirstOrDefault(a => a.Id == asignacion.Id);
                    if (existente != null)
                    {
                        // Mapeo manual de campos escalares
                        existente.IdConsultor = asignacion.IdConsultor;
                        existente.IdFrente = asignacion.IdFrente;
                        existente.IdSubFrente = asignacion.IdSubFrente;
                        existente.IdTipoActividad = asignacion.IdTipoActividad;
                        existente.FechaAsignacion = asignacion.FechaAsignacion;
                        existente.FechaDesasignacion = asignacion.FechaDesasignacion;
                        existente.Activo = asignacion.Activo;

                        if (frenteAsociado != null)
                        {
                            if (frenteAsociado.Id > 0) existente.IdTicketFrenteSubFrente = frenteAsociado.Id;
                            else existente.TicketFrenteSubFrente = frenteAsociado;
                        }

                        // Actualizar detalles de tareas (hijos)
                        if (asignacion.DetalleTareasConsultor != null)
                        {
                            foreach (var tarea in asignacion.DetalleTareasConsultor)
                            {
                                if (tarea.Id == 0)
                                {
                                    existente.DetalleTareasConsultor.Add(tarea);
                                }
                                else
                                {
                                    var tareaExistente = existente.DetalleTareasConsultor.FirstOrDefault(t => t.Id == tarea.Id);
                                    if (tareaExistente != null)
                                    {
                                        tareaExistente.IdTipoActividad = tarea.IdTipoActividad;
                                        tareaExistente.FechaInicio = tarea.FechaInicio;
                                        tareaExistente.FechaFin = tarea.FechaFin;
                                        tareaExistente.Horas = tarea.Horas;
                                        tareaExistente.Descripcion = tarea.Descripcion;
                                        tareaExistente.Activo = tarea.Activo;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return nuevosIdsConsultores;
    }
    public void VincularPlanificacionesConAsignaciones()
    {
        foreach (var frente in this.FrenteSubFrentes.Where(f => f.Activo))
        {
            if (frente.DetallePlanificacionConsultor == null) continue;

            var asignacionesFrente = this.ConsultorAsignaciones
                .Where(a => a.Activo && (a.IdTicketFrenteSubFrente == frente.Id || a.TicketFrenteSubFrente == frente))
                .ToList();

            if (!asignacionesFrente.Any())
            {
                // Si no hay asignaciones para este frente, la planificación queda huérfana de asignación (IdTicketConsultorAsignacion = null)
                foreach (var plan in frente.DetallePlanificacionConsultor.Where(p => p.Activo))
                {
                    plan.IdTicketConsultorAsignacion = null;
                    plan.TicketConsultorAsignacion = null;
                }
                continue;
            }

            foreach (var plan in frente.DetallePlanificacionConsultor.Where(p => p.Activo))
            {
                if (plan.IdTicketConsultorAsignacion.HasValue && plan.IdTicketConsultorAsignacion.Value > 0)
                {
                    var matchingAsig = asignacionesFrente.FirstOrDefault(a => a.Id == plan.IdTicketConsultorAsignacion.Value);
                    if (matchingAsig != null)
                    {
                        plan.TicketConsultorAsignacion = matchingAsig;
                    }
                }
                else
                {
                    var defaultAsig = asignacionesFrente.First();
                    if (defaultAsig.Id > 0)
                    {
                        plan.IdTicketConsultorAsignacion = defaultAsig.Id;
                    }
                    else
                    {
                        plan.TicketConsultorAsignacion = defaultAsig;
                    }
                }
            }
        }
    }
}

public class TicketConsultorAsignacion
{
    public int Id { get; set; }
    public int IdTicket { get; set; }
    public int? IdConsultor { get; set; }
    public int? IdFrente { get; set; }
    public int? IdSubFrente { get; set; }
    public int? IdTicketFrenteSubFrente { get; set; }
    public int IdTipoActividad { get; set; }
    public DateTime FechaAsignacion { get; set; }
    public DateTime FechaDesasignacion { get; set; }
    public bool Activo { get; set; } = true;
    public virtual Ticket Ticket { get; set; } = null!;
    [ForeignKey(nameof(IdConsultor))]
    public virtual Consultor Consultor { get; set; } = null!;
    public ICollection<DetalleTareasConsultor> DetalleTareasConsultor { get; set; } = new List<DetalleTareasConsultor>();
    public virtual ICollection<DetallePlanificacionConsultor> DetallePlanificacionConsultor { get; set; } = new List<DetallePlanificacionConsultor>();
    [ForeignKey(nameof(IdTicketFrenteSubFrente))]
    public virtual TicketFrenteSubFrente? TicketFrenteSubFrente { get; set; }
}
public class DetalleTareasConsultor
{
    public int Id { get; set; }
    public int IdTicketConsultorAsignacion { get; set; }
    public int IdTipoActividad { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal Horas { get; set; }
    public string Descripcion { get; set; }
    public bool Activo { get; set; } = true;
    public TicketConsultorAsignacion TicketConsultorAsignacion { get; set; }
}
public class DetallePlanificacionConsultor
{
    public int Id { get; set; }
    public int IdTicketFrenteSubFrente { get; set; }
    public int? IdTicketConsultorAsignacion { get; set; }
    public int IdTipoActividad { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal Horas { get; set; }
    public string Descripcion { get; set; }
    public bool Activo { get; set; } = true;
    public TicketFrenteSubFrente? TicketFrenteSubFrente { get; set; }
    [ForeignKey(nameof(IdTicketConsultorAsignacion))]
    public TicketConsultorAsignacion? TicketConsultorAsignacion { get; set; }
}
public class TicketFrenteSubFrente
{
    public int Id { get; set; }
    public int IdTicket { get; set; }
    public int IdFrente { get; set; }
    public int IdSubFrente { get; set; }
    public int Cantidad { get; set; }
    public string Descripcion { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string UsuarioCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? UsuarioModificacion { get; set; }
    public bool Activo { get; set; } = true;
    public virtual Ticket Ticket { get; set; } = null!;
    public virtual ICollection<DetallePlanificacionConsultor> DetallePlanificacionConsultor { get; set; } = new List<DetallePlanificacionConsultor>();
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