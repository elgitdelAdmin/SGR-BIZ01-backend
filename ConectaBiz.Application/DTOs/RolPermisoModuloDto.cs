using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.DTOs
{
    public class RolPermisoModuloDto
    {
        public int Id { get; set; }
        public int IdRol { get; set; }
        public int IdModulo { get; set; }

        public string DivsOcultos { get; set; } = "[]";
        public string ControlesBloqueados { get; set; } = "[]";
        public string ControlesOcultos { get; set; } = "[]";

        public Modulo Modulo { get; set; } = null!;
    }
}
