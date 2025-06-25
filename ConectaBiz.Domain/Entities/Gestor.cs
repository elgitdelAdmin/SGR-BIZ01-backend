using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Entities
{
    public class Gestor
    {
        public int Id { get; set; }
        public int PersonaId { get; set; }
        public int? IdNivelExperiencia { get; set; }
        public int? IdModalidadLaboral { get; set; }
        public int? IdSocio { get; set; }
        public int IdUser { get; set; }
        public string UsuarioCreacion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? UsuarioActualizacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public bool Activo { get; set; } = true;

        // Navegación
        public virtual Persona Persona { get; set; }
        public virtual Socio Socio { get; set; }
        public virtual ICollection<Empresa> Empresas { get; set; } = new List<Empresa>();
        public virtual ICollection<GestorFrenteSubFrente> GestorFrenteSubFrente { get; set; } = new List<GestorFrenteSubFrente>();
    }
}
