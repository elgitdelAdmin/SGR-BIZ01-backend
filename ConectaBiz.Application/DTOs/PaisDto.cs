using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.DTOs
{
    // DTOs para Pais
    public class PaisDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string? CodigoPostal { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioRegistro { get; set; }
        public string? UsuarioModificacion { get; set; }
    }

    public class CreatePaisDto
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string? CodigoPostal { get; set; }
        public string? UsuarioRegistro { get; set; }
    }

    public class UpdatePaisDto
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string? CodigoPostal { get; set; }
        public string? UsuarioModificacion { get; set; }
    }
}
