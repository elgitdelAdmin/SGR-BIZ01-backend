using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.DTOs
{
    public class EmpresaDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string? NombreComercial { get; set; }
        public string? NumDocContribuyente { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioRegistro { get; set; }
        public string? UsuarioModificacion { get; set; }
        public int? IdPais { get; set; }
        public string? NombrePais { get; set; }
        public int? IdGestor { get; set; }
        public string? NombreGestor { get; set; }
        public int IdSocio { get; set; }
        public string? NombreSocio { get; set; }
        public int IdPersonaResponsable { get; set; }
        public PersonaDto Persona { get; set; }
    }

    public class CreateEmpresaDto
    {
        //public string Codigo { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string? NombreComercial { get; set; }
        public string? NumDocContribuyente { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public bool Activo { get; set; } = true;
        public string? UsuarioRegistro { get; set; }
        public int? IdPais { get; set; }
        public int? IdGestor { get; set; }
        public int IdSocio { get; set; }
        public CreatePersonaDto Persona { get; set; }
    }

    public class UpdateEmpresaDto
    {
        public string RazonSocial { get; set; } = string.Empty;
        public string? NombreComercial { get; set; }
        public string? NumDocContribuyente { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public bool Activo { get; set; }
        public string? UsuarioModificacion { get; set; }
        public int? IdPais { get; set; }
        public int? IdGestor { get; set; }
        public int IdSocio { get; set; }
        public UpdatePersonaDto Persona { get; set; }
    }
}
