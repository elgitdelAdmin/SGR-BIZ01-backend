using System;

namespace ConectaBiz.Application.DTOs
{
    public class EmpresaGestorDto
    {
        public int Id { get; set; }
        public int IdEmpresa { get; set; }
        public int IdGestor { get; set; }
        public string? NombreGestor { get; set; }
        public bool EsPrincipal { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public DateTime? FechaDesasignacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public string? UsuarioModificacion { get; set; }
        public List<int> IdsTiposTicketPermitidos { get; set; } = new List<int>();
    }

    public class CrearEmpresaGestorDto
    {
        public int IdEmpresa { get; set; }
        public int IdGestor { get; set; }
        public bool EsPrincipal { get; set; } = false;
        public string? UsuarioCreacion { get; set; }
    }
}
