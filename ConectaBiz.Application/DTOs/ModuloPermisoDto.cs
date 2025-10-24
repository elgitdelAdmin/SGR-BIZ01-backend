using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.DTOs
{
    public class ModuloPermisoDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string? Icono { get; set; }
        public string? Ruta { get; set; }
        public List<string> DivsOcultos { get; set; } = new();
        public List<string> DivsBloqueados { get; set; } = new();
        public List<string> ControlesBloqueados { get; set; } = new();
        public List<string> ControlesOcultos { get; set; } = new();
    }

}
