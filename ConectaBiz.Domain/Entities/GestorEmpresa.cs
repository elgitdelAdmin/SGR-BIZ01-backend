using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Entities
{
    public class GestorEmpresa
    {
        public int Id { get; set; }
        public int IdGestor { get; set; }
        public int EmpresaId { get; set; }
        public string UsuarioCreacion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? UsuarioActualizacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public bool Activo { get; set; } = true;

        // Navegación
        public virtual Gestor Gestor { get; set; }
        public virtual Empresa Empresa { get; set; }
    }
}
