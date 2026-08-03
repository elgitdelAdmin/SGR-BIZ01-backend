

namespace ConectaBiz.Application.DTOs
{
    public class CargaMasivaDto
    {
        public string TipoCarga { get; set; }
        public FileUploadDto Excel { get; set; } = new FileUploadDto();
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
        public string GrupoAsignacion { get; set; }
        public string DatosCargaMasiva { get; set; }
    }

    public class TicketInsertMasivoDto
    {
        public string CodTicketInterno { get; set; }
        public string Titulo { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public int IdTipoTicket { get; set; }
        public int? IdSubTipoTicket { get; set; }
        public int IdEstadoTicket { get; set; }
        public int IdEmpresa { get; set; }
        public int? IdUsuarioResponsableCliente { get; set; }
        public int IdPrioridad { get; set; }
        public string Descripcion { get; set; }
        public string UsuarioCreacion { get; set; }
        public int? IdGestor { get; set; }
        public int? IdGestorConsultoria { get; set; }        
        public bool EsCargaMasiva { get; set; } = false;
        public string? DatosCargaMasiva { get; set; }
        public string? GrupoAsignacion { get; set; }
        public List<TicketConsultorAsignacionInsertDto> ConsultorAsignaciones { get; set; } = new List<TicketConsultorAsignacionInsertDto>();
        public List<TicketFrenteSubFrenteInsertDto> FrenteSubFrentes { get; set; } = new List<TicketFrenteSubFrenteInsertDto>();
    }
}
