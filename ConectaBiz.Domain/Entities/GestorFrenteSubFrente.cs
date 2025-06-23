using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Entities
{
    public class GestorFrenteSubFrente
    {
        public int Id { get; set; }
        public int IdGestor { get; set; }
        public int IdFrente { get; set; }
        public int IdSubFrente { get; set; }
        public int? IdNivelExperiencia { get; set; }
        public bool EsCertificado { get; set; } = false;
        public DateTime FechaCreacion { get; set; }
        public string UsuarioCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public string? UsuarioActualizacion { get; set; }
        public bool Activo { get; set; } = true;

        // Navegación
        public virtual Gestor Gestor { get; set; }
        public virtual Frente Frente { get; set; }
        public virtual SubFrente SubFrente { get; set; }
    }
}
