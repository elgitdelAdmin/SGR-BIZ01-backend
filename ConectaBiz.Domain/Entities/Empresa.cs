using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Entities
{
    public class Empresa
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string? NombreComercial { get; set; }
        public string? NumDocContribuyente { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioRegistro { get; set; }
        public string? UsuarioModificacion { get; set; }
        public int? IdPais { get; set; }
        public int? IdGestor { get; set; }
        public int IdSocio { get; set; }
        public int IdPersonaResponsable { get; set; }
        public string? CargoResponsable { get; set; }

        // Propiedades de navegación
        public virtual Pais? Pais { get; set; }
        public virtual Gestor? Gestor { get; set; }
        public virtual Socio? Socio { get; set; }
        public virtual Persona? PersonaResponsable { get; set; }
    }

}
