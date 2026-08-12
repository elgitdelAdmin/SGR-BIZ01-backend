using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Entities
{
    public class Consultor
    {
        public Consultor()
        {
            Activo = true;
            FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
        }

        public int Id { get; set; }
        public int PersonaId { get; set; }
        public int? IdNivelExperiencia { get; set; }
        public int? IdModalidadLaboral { get; set; }
        // public int? IdSocio { get; set; } // Comentado por instrucción del usuario
        public int IdUser { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public string UsuarioCreacion { get; set; }
        public string? UsuarioActualizacion { get; set; }
        public bool Activo { get; set; }

        // Propiedades de navegación
        public virtual Persona Persona { get; set; }
        // public Socio Socio { get; set; } // Comentado por instrucción del usuario
        public virtual List<ConsultorFrenteSubFrente> ConsultorFrenteSubFrente { get; set; } = new List<ConsultorFrenteSubFrente>();

        // Métodos de Dominio (Reglas de Negocio)
        public void ActualizarDatosBasicos(int? idNivelExperiencia, int? idModalidadLaboral, int? idSocio, string usuarioActualizacion)
        {
            this.IdNivelExperiencia = idNivelExperiencia;
            this.IdModalidadLaboral = idModalidadLaboral;
            // this.IdSocio = idSocio; // Comentado por instrucción del usuario
            this.UsuarioActualizacion = usuarioActualizacion;
            this.FechaActualizacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
        }

        public void ValidarEspecializacionesNuevas(IEnumerable<ConsultorFrenteSubFrente> nuevasEspecializaciones)
        {
            var duplicados = nuevasEspecializaciones
                .GroupBy(e => new { e.IdFrente, e.IdSubFrente})
                .Where(g => g.Count() > 1);

            if (duplicados.Any())
            {
                throw new InvalidOperationException("Se encontraron especializaciones duplicadas. Un consultor no puede tener la misma especialización repetida.");
            }
        }

        public bool EspecializacionesSonDiferentes(IEnumerable<ConsultorFrenteSubFrente> nuevas)
        {
            if (this.ConsultorFrenteSubFrente.Count != nuevas.Count())
                return true;

            foreach (var actual in this.ConsultorFrenteSubFrente)
            {
                var existe = nuevas.Any(nueva =>
                    nueva.IdFrente == actual.IdFrente &&
                    nueva.IdSubFrente == actual.IdSubFrente);

                if (!existe) return true;
            }

            return false;
        }
    }
}
