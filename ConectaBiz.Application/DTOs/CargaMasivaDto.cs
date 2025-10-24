using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConectaBiz.Application.DTOs
{
    public class CargaMasivaDto
    {
        public string TipoCarga { get; set; }

        // Esto le dice a Swagger que es un archivo desde el form
        [FromForm(Name = "excel")]
        public IFormFile Excel { get; set; }
    }
    public class CargaMasivaGenericoDto
    {
        public string CodTicket { get; set; }
        public string Titulo { get; set; }
        public string FechaSolicitud { get; set; }
        public string EstadoTicket { get; set; }
        public string IdPrioridad { get; set; }
        public string Descripcion { get; set; }
        public string UsuarioCreacion { get; set; }
        public string Asignado { get; set; }
        public string DatosCargaMasiva { get; set; }
    }
    public class CargaMasivaIncidentesAlicorpDto
    {
        public string Numero { get; set; }
        public string Solicitante { get; set; }
        public string UsuarioFinalAfectado { get; set; }
        public string Canal { get; set; }
        public string Estado { get; set; }
        public string MotivosParaPonerEnEspera { get; set; }
        public string BreveDescripcion { get; set; }
        public string Descripcion { get; set; }
        public string Prioridad { get; set; }
        public string Urgencia { get; set; }
        public string Impacto { get; set; }
        public string SedeDelIncidente { get; set; }
        public string Sociedad { get; set; }
        public string ElementoDeConfiguracion { get; set; }
        public string Servicio { get; set; }
        public string Categoria1 { get; set; }
        public string Categoria2 { get; set; }
        public string Categoria3 { get; set; }
        public string GrupoDeAsignacion { get; set; }
        public string AsignadoA { get; set; }
        public string TicketExterno { get; set; }
        public string Creados { get; set; }
        public string CreadosPor { get; set; }
        public string Actualizados { get; set; }
        public string ActualizadoPor { get; set; }
        public string NivelFuncional { get; set; }
    }
    public class CargaMasivaRequerimientosAlicorpDto
    {
        public string Numero { get; set; }
        public string Solicitante { get; set; }
        public string UsuarioAfectado { get; set; }
        public string Estado { get; set; }
        public string MotivosParaPonerEnEspera { get; set; }
        public string Canal { get; set; }
        public string Prioridad { get; set; }
        public string Elemento { get; set; }
        public string Servicio { get; set; }
        public string Categoria1 { get; set; }
        public string Categoria2 { get; set; }
        public string Categoria3 { get; set; }
        public string BreveDescripcion { get; set; }
        public string Descripcion { get; set; }
        public string Sede { get; set; }
        public string GrupoDeAsignacion { get; set; }
        public string AsignadoA { get; set; }
        public string TicketExterno { get; set; }
        public string Creados { get; set; }
        public string CreadosPor { get; set; }
        public string Actualizados { get; set; }
        public string ActualizadoPor { get; set; }
        public string NivelFuncional { get; set; }
    }
    public class CargaMasivaIncidentesExceliaDto
    {
        public string Ticket { get; set; }
        public string Opened { get; set; }
        public string ShortDescription { get; set; }
        public string Caller { get; set; }
        public string Transaccion { get; set; }
        public string Priority { get; set; }
        public string State { get; set; }
        public string Category1 { get; set; }
        public string Category2 { get; set; }
        public string AssignmentGroup { get; set; }
        public string AssignedTo { get; set; }
        public string Updated { get; set; }
        public string UpdatedBy { get; set; }
        public string Category3 { get; set; }
        public string CerradoPor { get; set; }
        public string Company { get; set; }
        public string CreatedBy { get; set; }
        public string Duration { get; set; }
        public string DurationOpened { get; set; }
        public string DurationPending { get; set; }
        public string DiasPendientes { get; set; }
        public string FechaUltimoCambioDeGrupo { get; set; }
        public string MadeSLA { get; set; }
        public string NotasDeTrabajo { get; set; }
        public string NotasDeResolucion { get; set; }
        public string ResolveTime { get; set; }
        public string ResolvedBy { get; set; }
        public string TimeWorked { get; set; }
        public string Urgency { get; set; }
        public string UsuarioSolicitante { get; set; }
        public string Escalation { get; set; }
        public string MotivoDePendiente { get; set; }
        public string Domain { get; set; }
        public string Closed { get; set; }
        public string Impact { get; set; }
        public string MayorIncident { get; set; }
        public string Severity { get; set; }
        public string CopiaDeCategoria1 { get; set; }
        public string BusinessService { get; set; }
        public string DueDate { get; set; }
        public string SLA { get; set; }
        public string SLADue { get; set; }
        public string RequestUser { get; set; }
        public string OpenedBy { get; set; }
        public string SAPSociety { get; set; }
        public string CodigoDeCierre { get; set; }
        public string Resolved { get; set; }
        public string NivelFuncional { get; set; }
    }
    public class CargaMasivaTicketsRansaDto
    {
        public string IdTicket { get; set; }
        public string TipoDeTicket { get; set; }
        public string TicketRelacionado { get; set; }
        public string Fuente { get; set; }
        public string Solicitante { get; set; }
        public string CorreoElectronicoSolicitante { get; set; }
        public string UsuarioAfectado { get; set; }
        public string CorreoUsuarioAfectado { get; set; }
        public string VIP { get; set; }
        public string ServiciosAfectados { get; set; }
        public string Clase { get; set; }
        public string Categoria { get; set; }
        public string Tipo { get; set; }
        public string Elemento { get; set; }
        public string Impacto { get; set; }
        public string Urgencia { get; set; }
        public string Prioridad { get; set; }
        public string FechaDeCreacion { get; set; }
        public string Mes { get; set; }
        public string FechaDeResolucion { get; set; }
        public string FechaDeCierre { get; set; }
        public string AsignarAlGrupo { get; set; }
        public string AsignarAlIndividuo { get; set; }
        public string AutorDeLaCreacion { get; set; }
        public string Estado { get; set; }
        public string Motivo { get; set; }
        public string AntiguedadDelTicketDias { get; set; }
        public string Descripcion { get; set; }
        public string Detalles { get; set; }
        public string UbicacionId139 { get; set; }
        public string CodigoDeCierre { get; set; }
        public string CandidatoProblema { get; set; }
    }
    public class TicketInsertMasivoDto
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
        public string UsuarioCreacion { get; set; }
        public int? IdGestor { get; set; }
        public bool EsCargaMasiva { get; set; } = false;
        public string? DatosCargaMasiva { get; set; }
        public List<TicketConsultorAsignacionInsertDto> ConsultorAsignaciones { get; set; } = new List<TicketConsultorAsignacionInsertDto>();
    }
}
