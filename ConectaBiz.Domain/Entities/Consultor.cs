using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Entities
{
    public class Consultor
    {
        public int Id { get; set; }
        public int PersonaId { get; set; }
        public int IdNivelExperiencia { get; set; }
        public int IdModalidadLaboral { get; set; }
        public int IdSocio { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public bool Activo { get; set; }

        // Propiedades de navegación
        public virtual Persona Persona { get; set; }
        public Socio Socio { get; set; }
        public virtual List<ConsultorFrenteSubFrente> ConsultorFrenteSubFrente { get; set; } = new List<ConsultorFrenteSubFrente>();
    }
}
