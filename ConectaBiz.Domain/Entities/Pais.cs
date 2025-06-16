using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Entities
{
    public class Pais
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string? CodigoPostal { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioRegistro { get; set; }
        public string? UsuarioModificacion { get; set; }

        // Navegación
        public virtual ICollection<Empresa> Empresas { get; set; } = new List<Empresa>();
    }
}
