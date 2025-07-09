using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int IdRol { get; set; }
        public SocioDto Socio { get; set; }
        public PersonaDto Persona { get; set; }
        public RolDto Rol { get; set; }
    }

    public class LoginRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int IdSocio { get; set; }
        public int IdRol { get; set; }
        public string UsuarioCreacion { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public CreatePersonaDto Persona { get; set; }
    }
    public class UpdateUserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; } // Opcional para actualizar contraseña
        //public int IdSocio { get; set; }
        //public int IdRol { get; set; }
        public string UsuarioActualizacion { get; set; } = string.Empty;
        public UpdatePersonaDto? Persona { get; set; }
    }
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; } = new UserDto();
    }

    public class RefreshTokenRequestDto
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
    // DTO para representar la información de Persona
    public class PersonaDto
    {
        public int Id { get; set; }
        public string Nombres { get; set; }
        public string ApellidoMaterno { get; set; }
        public string ApellidoPaterno { get; set; }
        public string? NumeroDocumento { get; set; }
        public int TipoDocumento { get; set; }
        public string? Telefono { get; set; }
        public string? Telefono2 { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public bool Activo { get; set; }
    }
    public class PersonaConUsuariosEmpresaDto
    {
        public int Id { get; set; }
        public string Nombres { get; set; }
        public string ApellidoMaterno { get; set; }
        public string ApellidoPaterno { get; set; }
        public string? NumeroDocumento { get; set; }
        public int TipoDocumento { get; set; }
        public string? Telefono { get; set; }
        public string? Telefono2 { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public bool Activo { get; set; }

        public List<UserEmpresaDto> Users { get; set; } = new();
    }
    public class UserEmpresaDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RolCodigo { get; set; } = string.Empty;
    }

    // DTO específico para crear una persona (campos mínimos necesarios)
    public class CreatePersonaDto
    {
        public string Nombres { get; set; }
        public string ApellidoMaterno { get; set; }
        public string ApellidoPaterno { get; set; }
        public string? NumeroDocumento { get; set; }
        public int TipoDocumento { get; set; }
        public string? Telefono { get; set; }
        public string? Telefono2 { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string UsuarioCreacion { get; set; }
    }
    public class UpdatePersonaDto
    {
        public int Id { get; set; }
        public string Nombres { get; set; }
        public string ApellidoMaterno { get; set; }
        public string ApellidoPaterno { get; set; }
        public string? NumeroDocumento { get; set; }
        public int TipoDocumento { get; set; }
        public string? Telefono { get; set; }
        public string? Telefono2 { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string UsuarioActualizacion { get; set; }
    }
    public class RolDto
    {
        public int Id { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Activo { get; set; }
    }

}
