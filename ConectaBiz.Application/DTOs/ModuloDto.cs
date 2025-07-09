using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.DTOs
{
    public class ModuloDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string? Icono { get; set; }
        public string? Ruta { get; set; }
        public bool Activo { get; set; }
        public virtual List<RolPermisoModulo> RolPermisoModulos { get; set; } = new List<RolPermisoModulo>();
    }
}
