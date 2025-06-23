using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Entities
{
    public class ConsultorFrenteSubFrente
    {
        public int Id { get; set; }
        public int ConsultorId { get; set; }
        public int IdFrente { get; set; }
        public int IdSubFrente { get; set; }
        public int IdNivelExperiencia { get; set; }
        public bool EsCertificado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public bool Activo { get; set; }

        // Propiedades de navegación
        public virtual Consultor Consultor { get; set; }
        public virtual Frente Frente { get; set; }
        public virtual SubFrente SubFrente { get; set; }
    }
}
