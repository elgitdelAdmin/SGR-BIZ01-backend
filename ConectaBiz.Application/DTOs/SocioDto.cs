using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.DTOs
{
    public class SocioDto
    {
        public int Id { get; set; }
        public string RazonSocial { get; set; }
        public string? Codigo { get; set; }
        public string Nombre { get; set; }
        public string NombreComercial { get; set; }
        public string NumDocContribuyente { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono1 { get; set; }
        public string Telefono2 { get; set; }
        public string? Email { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioRegistro { get; set; }
        public string UsuarioModificacion { get; set; }

        // Si necesitas mostrar los usuarios relacionados:
        //public List<UserDto> Users { get; set; }
    }
}
