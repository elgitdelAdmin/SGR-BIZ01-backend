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
    public virtual ICollection<TicketGestorAsignacion> GestorAsignaciones { get; set; } = new List<TicketGestorAsignacion>();

    public static Ticket CrearDesdeCargaMasiva(
        string codTicket,
        string codTicketInterno,
        string titulo,
        DateTime fechaSolicitud,
        int idTipoTicket,
        int? idSubTipoTicket,
        int idEstadoTicket,
        int idEmpresa,
        int idUsuarioResponsableCliente,
        int idPrioridad,
        string? descripcion,
        int? idGestorConsultoria,
        string? datosCargaMasiva,
        List<TicketConsultorAsignacion>? asignaciones,
        List<TicketFrenteSubFrente>? frentesSubFrentes)
    {
        var ticket = new Ticket
        {
            CodTicket = codTicket,
            CodTicketInterno = codTicketInterno,
            Titulo = titulo,
            FechaSolicitud = DateTime.SpecifyKind(fechaSolicitud, DateTimeKind.Local),
            IdTipoTicket = idTipoTicket,
            IdSubTipoTicket = idSubTipoTicket,
            IdEmpresa = idEmpresa,
            IdUsuarioResponsableCliente = idUsuarioResponsableCliente,
            IdPrioridad = idPrioridad,
            Descripcion = descripcion,
            UsuarioCreacion = "CargaMasivaExcel",
            Activo = true,
            FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
            EsCargaMasiva = true,
            IdGestorConsultoria = idGestorConsultoria,
            DatosCargaMasiva = datosCargaMasiva
        };

        if (frentesSubFrentes != null)
        {
            foreach (var fsf in frentesSubFrentes)
                ticket.FrenteSubFrentes.Add(fsf);
        }

        if (asignaciones != null)
        {
            foreach (var asig in asignaciones)
                ticket.ConsultorAsignaciones.Add(asig);
        }

        foreach (var ca in ticket.ConsultorAsignaciones)
        {
            if (ca.IdSubFrente.HasValue)
            {
                var matchingFsf = ticket.FrenteSubFrentes
                    .FirstOrDefault(fsf => fsf.IdSubFrente == ca.IdSubFrente.Value);
                if (matchingFsf != null)
                    ca.TicketFrenteSubFrente = matchingFsf;
            }
        }

        ticket.InicializarEstado(idEstadoTicket, "CargaMasivaExcel");

        return ticket;
    }

    public static Ticket Crear(
        string codTicket,
        string titulo,
        DateTime fechaSolicitud,
        int idTipoTicket,
        int? idSubTipoTicket,
        int idEstadoInicial,
        int idEmpresa,
        int idUsuarioResponsableCliente,
        int idPrioridad,
        int? idGestorConsultoria,
        string? descripcion,
        string? repositorios,
        string usuarioCreacion,
        int? idGestorPrincipalEmpresa,
        List<int> gestoresEmpresaActivos,
        List<int> idGestoresSecundarios,
        string? codTicketInterno = null,
        string? codReqSgrCsti = null,
        int? idReqSgrCsti = null,
        bool? esCargaMasiva = null)
    {
        var ticket = new Ticket
        {
            CodTicket = codTicket,
            CodTicketInterno = string.IsNullOrEmpty(codTicketInterno) ? "" : codTicketInterno,
            CodReqSgrCsti = codReqSgrCsti,
            IdReqSgrCsti = idReqSgrCsti,
            EsCargaMasiva = esCargaMasiva ?? false,
            Titulo = titulo,
            FechaSolicitud = DateTime.SpecifyKind(fechaSolicitud, DateTimeKind.Local),
            IdTipoTicket = idTipoTicket,
            IdSubTipoTicket = idSubTipoTicket,
            IdEmpresa = idEmpresa,
            IdUsuarioResponsableCliente = idUsuarioResponsableCliente,
            IdPrioridad = idPrioridad,
            Descripcion = descripcion,
            IdGestorConsultoria = idGestorConsultoria,
            Repositorios = repositorios,
            UsuarioCreacion = usuarioCreacion,
            Activo = true,
            FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local)
        };

        ticket.InicializarEstado(idEstadoInicial, usuarioCreacion);

        if (idGestoresSecundarios != null && idGestoresSecundarios.Any())
        {
            foreach (var idGestor in idGestoresSecundarios)
            {
                if (gestoresEmpresaActivos.Contains(idGestor)) continue;
                
                ticket.GestorAsignaciones.Add(new TicketGestorAsignacion
                {
                    IdGestor = idGestor,
                    IdGestorAsigno = idGestorPrincipalEmpresa ?? 0,
                    Activo = true,
                    FechaAsignacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
                    FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
                    UsuarioCreacion = usuarioCreacion
                });
            }
        }

        return ticket;
    }

    public static Ticket CrearRapido(
        string codTicket,
        string titulo,
        DateTime fechaSolicitud,
        int idTipoTicket,
        int? idSubTipoTicket,
        int idEstadoInicial,
        int idEmpresa,
        int idUsuarioResponsableCliente,
        int idPrioridad,
        int? idGestorConsultoria,
        string? descripcion,
        string? repositorios,
        string usuarioCreacion,
        int? idGestorPrincipalEmpresa,
        List<int> gestoresEmpresaActivos,
        List<int> idGestoresSecundarios,
        List<TicketConsultorAsignacion> asignaciones,
        List<TicketFrenteSubFrente> frentesSubFrentes,
        string? codTicketInterno = null,
        string? codReqSgrCsti = null,
        int? idReqSgrCsti = null,
        bool? esCargaMasiva = null)
    {
        if (frentesSubFrentes == null || !frentesSubFrentes.Any())
        {
            throw new InvalidOperationException("Debe agregar al menos una especialización para la creación rápida de ticket.");
        }

        foreach (var frente in frentesSubFrentes)
        {
            if (frente.DetallePlanificacionConsultor == null || !frente.DetallePlanificacionConsultor.Any())
            {
                throw new InvalidOperationException($"La especialización configurada debe tener al menos un registro de planificación de horas.");
            }
        }

        if (asignaciones == null || !asignaciones.Any(a => a.IdConsultor.HasValue && a.IdConsultor > 0))
        {
            throw new InvalidOperationException("Debe asignar al menos un consultor para la creación rápida de ticket.");
        }

        var ticket = Crear(
            codTicket, titulo, fechaSolicitud, idTipoTicket, idSubTipoTicket, idEstadoInicial, idEmpresa, idUsuarioResponsableCliente,
            idPrioridad, idGestorConsultoria, descripcion, repositorios, usuarioCreacion, idGestorPrincipalEmpresa, gestoresEmpresaActivos, 
            idGestoresSecundarios, codTicketInterno, codReqSgrCsti, idReqSgrCsti, esCargaMasiva);

        if (frentesSubFrentes != null)
        {
            foreach (var fsf in frentesSubFrentes)
            {
                fsf.FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                fsf.UsuarioCreacion = usuarioCreacion;
                ticket.FrenteSubFrentes.Add(fsf);
            }
        }

        if (asignaciones != null)
        {
            foreach (var asig in asignaciones)
                ticket.ConsultorAsignaciones.Add(asig);
        }

        foreach (var ca in ticket.ConsultorAsignaciones)
        {
            if (ca.IdSubFrente.HasValue)
            {
                var matchingFsf = ticket.FrenteSubFrentes
                    .FirstOrDefault(fsf => fsf.IdSubFrente == ca.IdSubFrente.Value);
                if (matchingFsf != null)
                    ca.TicketFrenteSubFrente = matchingFsf;
            }
        }

        return ticket;
    }

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
    public void CambiarEstado(Parametro estadoActual, Parametro estadoNuevo, string usuario)
    {
        if (estadoActual == null || estadoNuevo == null)
            throw new ArgumentException("Los parámetros de estado no pueden ser nulos.");

        // Validamos con el Valor1
        var transicionesPermitidas = estadoActual.Valor1?.Split(',') ?? Array.Empty<string>();

        if (!transicionesPermitidas.Contains(estadoNuevo.Codigo))
        {
            throw new InvalidOperationException($"Regla de Negocio: No se puede pasar un ticket de {estadoActual.Codigo} a {estadoNuevo.Codigo}.");
        }

        // Si pasó la validación, llamamos a tu método original para que haga el historial
        this.CambiarEstado(estadoNuevo.Id, usuario);
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
    public (int Agregados, int Modificados, int Eliminados) ActualizarFrentes(IEnumerable<TicketFrenteSubFrente> frentesNuevos, string usuarioActualizacion)
    {
        int agregados = 0;
        int modificados = 0;
        int eliminados = 0;

        if (frentesNuevos == null) return (0, 0, 0);

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
                    bool recienEliminado = existente.Activo && !frente.Activo;
                    bool modificado = existente.IdFrente != frente.IdFrente ||
                                      existente.IdSubFrente != frente.IdSubFrente ||
                                      existente.Cantidad != frente.Cantidad ||
                                      existente.Descripcion != frente.Descripcion ||
                                      existente.FechaInicio != frente.FechaInicio ||
                                      existente.FechaFin != frente.FechaFin;

                    existente.IdFrente = frente.IdFrente;
                    existente.IdSubFrente = frente.IdSubFrente;
                    existente.Cantidad = frente.Cantidad;
                    existente.Descripcion = frente.Descripcion;
                    existente.FechaInicio = frente.FechaInicio;
                    existente.FechaFin = frente.FechaFin;
                    existente.Activo = frente.Activo;

                    if (modificado || recienEliminado)
                    {
                        existente.UsuarioModificacion = usuarioActualizacion;
                        existente.FechaModificacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                    }

                    if (frente.Activo && frente.DetallePlanificacionConsultor != null)
                    {
                        foreach (var planNuevo in frente.DetallePlanificacionConsultor)
                        {
                            if (planNuevo.Id == 0)
                            {
                                existente.DetallePlanificacionConsultor.Add(planNuevo);
                                modificado = true;
                            }
                            else
                            {
                                var planExistente = existente.DetallePlanificacionConsultor.FirstOrDefault(p => p.Id == planNuevo.Id);
                                if (planExistente != null)
                                {
                                    if (planExistente.IdTipoActividad != planNuevo.IdTipoActividad ||
                                        planExistente.FechaInicio != planNuevo.FechaInicio ||
                                        planExistente.FechaFin != planNuevo.FechaFin ||
                                        planExistente.Horas != planNuevo.Horas ||
                                        planExistente.Descripcion != planNuevo.Descripcion ||
                                        planExistente.Activo != planNuevo.Activo ||
                                        (planNuevo.IdTicketConsultorAsignacion > 0 && planExistente.IdTicketConsultorAsignacion != planNuevo.IdTicketConsultorAsignacion))
                                    {
                                        modificado = true;
                                    }

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
                    if (recienEliminado)
                    {
                        eliminados++;
                    }
                    else if (modificado)
                    {
                        modificados++;
                    }
                }
            }
        }
        return (agregados, modificados, eliminados);
    }
    public (HashSet<int> NuevosIdsConsultores, int Agregados, int Modificados, int Eliminados) ActualizarAsignaciones(IEnumerable<TicketConsultorAsignacion> asignacionesNuevas)
    {
        var nuevosIdsConsultores = new HashSet<int>();
        int agregados = 0;
        int modificados = 0;
        int eliminados = 0;

        if (asignacionesNuevas == null) return (nuevosIdsConsultores, agregados, modificados, eliminados);

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
                    agregados++;

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
                        bool recienEliminado = existente.Activo && !asignacion.Activo;
                        bool modificado = existente.IdConsultor != asignacion.IdConsultor ||
                                          existente.IdFrente != asignacion.IdFrente ||
                                          existente.IdSubFrente != asignacion.IdSubFrente ||
                                          existente.IdTipoActividad != asignacion.IdTipoActividad ||
                                          existente.FechaAsignacion != asignacion.FechaAsignacion ||
                                          existente.FechaDesasignacion != asignacion.FechaDesasignacion ||
                                          existente.Rechazado != asignacion.Rechazado ||
                                          existente.MotivoRechazo != asignacion.MotivoRechazo;

                        // Mapeo manual de campos escalares
                        existente.IdConsultor = asignacion.IdConsultor;
                        existente.IdFrente = asignacion.IdFrente;
                        existente.IdSubFrente = asignacion.IdSubFrente;
                        existente.IdTipoActividad = asignacion.IdTipoActividad;
                        existente.FechaAsignacion = asignacion.FechaAsignacion;
                        existente.FechaDesasignacion = asignacion.FechaDesasignacion;
                        existente.Activo = asignacion.Activo;
                        existente.Rechazado = asignacion.Rechazado;
                        existente.MotivoRechazo = asignacion.MotivoRechazo;
                        existente.FechaRechazo = asignacion.FechaRechazo;

                        if (frenteAsociado != null)
                        {
                            if (frenteAsociado.Id > 0 && existente.IdTicketFrenteSubFrente != frenteAsociado.Id)
                            {
                                existente.IdTicketFrenteSubFrente = frenteAsociado.Id;
                                modificado = true;
                            }
                            else if (existente.TicketFrenteSubFrente != frenteAsociado)
                            {
                                existente.TicketFrenteSubFrente = frenteAsociado;
                                modificado = true;
                            }
                        }

                        // Actualizar detalles de tareas (hijos)
                        if (asignacion.DetalleTareasConsultor != null)
                        {
                            foreach (var tarea in asignacion.DetalleTareasConsultor)
                            {
                                if (tarea.Id == 0)
                                {
                                    existente.DetalleTareasConsultor.Add(tarea);
                                    modificado = true;
                                }
                                else
                                {
                                    var tareaExistente = existente.DetalleTareasConsultor.FirstOrDefault(t => t.Id == tarea.Id);
                                    if (tareaExistente != null)
                                    {
                                        if (tareaExistente.IdTipoActividad != tarea.IdTipoActividad ||
                                            tareaExistente.FechaInicio != tarea.FechaInicio ||
                                            tareaExistente.FechaFin != tarea.FechaFin ||
                                            tareaExistente.Horas != tarea.Horas ||
                                            tareaExistente.Descripcion != tarea.Descripcion ||
                                            tareaExistente.Activo != tarea.Activo)
                                        {
                                            modificado = true;
                                        }

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
                        bool recienRechazado = !existente.Rechazado && asignacion.Rechazado;
                        if (recienEliminado || recienRechazado) eliminados++;
                        else if (modificado) modificados++;
                    }
                }
            }
        }
        return (nuevosIdsConsultores, agregados, modificados, eliminados);
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

    public void ActualizarGestoresSecundarios(List<int> idsGestoresSecundarios, int idGestorAccion, string usuarioAccion)
    {
        if (idsGestoresSecundarios == null) return;

        var gestoresEmpresaIds = this.Empresa != null && this.Empresa.EmpresaGestores != null
            ? this.Empresa.EmpresaGestores.Where(eg => eg.Activo).Select(eg => eg.IdGestor).ToList()
            : new List<int>();

        if (this.Empresa?.IdGestor != null && !gestoresEmpresaIds.Contains(this.Empresa.IdGestor.Value))
        {
            gestoresEmpresaIds.Add(this.Empresa.IdGestor.Value);
        }

        // Desactivar los que ya no están en la lista (que no pertenezcan a la empresa)
        foreach (var asig in this.GestorAsignaciones.Where(a => a.Activo))
        {
            if (!idsGestoresSecundarios.Contains(asig.IdGestor) && !gestoresEmpresaIds.Contains(asig.IdGestor))
            {
                asig.Activo = false;
                asig.FechaDesasignacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                asig.IdGestorDesasigno = idGestorAccion;
                asig.FechaModificacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                asig.UsuarioModificacion = usuarioAccion;
            }
        }

        // Agregar los nuevos (excluyendo a los que ya pertenecen a la empresa)
        foreach (var idGestor in idsGestoresSecundarios)
        {
            // Ignorar al gestor si ya pertenece a la empresa
            if (gestoresEmpresaIds.Contains(idGestor)) continue;

            var existente = this.GestorAsignaciones.FirstOrDefault(a => a.IdGestor == idGestor && a.Activo);
            if (existente == null)
            {
                this.GestorAsignaciones.Add(new TicketGestorAsignacion
                {
                    IdTicket = this.Id,
                    IdGestor = idGestor,
                    IdGestorAsigno = idGestorAccion,
                    Activo = true,
                    FechaAsignacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
                    FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
                    UsuarioCreacion = usuarioAccion
                });
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

    // Nuevos campos
    public bool Rechazado { get; set; } = false;
    public string? MotivoRechazo { get; set; }
    public DateTime? FechaRechazo { get; set; }

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

public class TicketGestorAsignacion
{
    public int Id { get; set; }
    public int IdTicket { get; set; }
    public int IdGestor { get; set; }
    public int IdGestorAsigno { get; set; }
    public int? IdGestorDesasigno { get; set; }
    public DateTime FechaAsignacion { get; set; } = DateTime.Now;
    public DateTime? FechaDesasignacion { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaModificacion { get; set; }
    public string? UsuarioCreacion { get; set; }
    public string? UsuarioModificacion { get; set; }

    [ForeignKey(nameof(IdTicket))]
    public virtual Ticket Ticket { get; set; } = null!;
    
    [ForeignKey(nameof(IdGestor))]
    public virtual Gestor Gestor { get; set; } = null!;
    
    [ForeignKey(nameof(IdGestorAsigno))]
    public virtual Gestor GestorAsigno { get; set; } = null!;
    
    [ForeignKey(nameof(IdGestorDesasigno))]
    public virtual Gestor? GestorDesasigno { get; set; }
}