using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Entities
{
    public class Frente
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioRegistro { get; set; }
        public string? UsuarioModificacion { get; set; }

        // Propiedades de navegación
        public virtual ICollection<SubFrente> SubFrente { get; set; } = new List<SubFrente>();
        public virtual ICollection<ConsultorFrenteSubFrente> ConsultorFrenteSubFrente { get; set; } = new List<ConsultorFrenteSubFrente>();
    }

    // Entities/SubFrente.cs
    public class SubFrente
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public int IdFrente { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioRegistro { get; set; }
        public string? UsuarioModificacion { get; set; }
        public string? Nivel { get; set; }

        // Propiedades de navegación
        public virtual Frente Frente { get; set; }
        public virtual ICollection<ConsultorFrenteSubFrente> ConsultorFrenteSubFrente { get; set; } = new List<ConsultorFrenteSubFrente>();
    }
}
