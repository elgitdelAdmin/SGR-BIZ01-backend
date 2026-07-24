using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConectaBiz.Domain.Entities
{
    [Table("EmpresaGestor", Schema = "conectabiz")]
    public class EmpresaGestor
    {
        public int Id { get; set; }
        public int IdEmpresa { get; set; }
        public int IdGestor { get; set; }
        public bool EsPrincipal { get; set; } = false;
        public bool Activo { get; set; } = true;
        public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaDesasignacion { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public string? UsuarioModificacion { get; set; }

        // Propiedades de navegación
        public virtual Empresa Empresa { get; set; } = null!;
        public virtual Gestor Gestor { get; set; } = null!;
    }
}
