using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Entities
{
    public class Modulo
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string? Icono { get; set; }
        public string? Ruta { get; set; }
        public bool Activo { get; set; }

        public ICollection<RolPermisoModulo> Permisos { get; set; } = new List<RolPermisoModulo>();
    }
}
