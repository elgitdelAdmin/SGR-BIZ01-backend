using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.DTOs
{
    public class ConsultorFrenteSubFrenteDto
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

        // Propiedades de navegación para mostrar información relacionada
        //public ConsultorDto? Consultor { get; set; }
        public FrenteDto? Frente { get; set; }
        public SubFrenteDto? SubFrente { get; set; }
    }
}