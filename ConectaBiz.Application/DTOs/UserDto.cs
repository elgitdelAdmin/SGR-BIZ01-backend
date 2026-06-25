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
        public SocioDto Socio { get; set; }
        public PersonaDto Persona { get; set; }
        public List<UserRolSocioDto> RolSocios { get; set; } = new List<UserRolSocioDto>();
    }

    public class UserRolSocioDto
    {
        public int IdRol { get; set; }
        public int IdSocio { get; set; }
        public string RolNombre { get; set; } = string.Empty;
        public string RolCodigo { get; set; } = string.Empty;
        public string SocioNombre { get; set; } = string.Empty;
    }

    public class RolSocioItemDto
    {
        public int IdRol { get; set; }
        public int IdSocio { get; set; }
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
        public List<RolSocioItemDto> RolSocios { get; set; } = new List<RolSocioItemDto>();
        public string UsuarioCreacion { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public CreatePersonaDto Persona { get; set; }
    }
    public class UpdateUserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
        public int IdSocio { get; set; }
        public List<RolSocioItemDto> RolSocios { get; set; } = new List<RolSocioItemDto>();
        public string UsuarioActualizacion { get; set; } = string.Empty;
        public UpdatePersonaDto? Persona { get; set; }
    }
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public int? IdConsultor { get; set; }
        public UserDto User { get; set; } = new UserDto();
        public List<NotificacionTicketDto>? NotificacionTicket { get; set; } = new List<NotificacionTicketDto>();
        public bool RequiereSeleccionRol { get; set; } = false;
        public List<UserRolSocioDto>? RolSociosDisponibles { get; set; }
        // Rol y socio seleccionados (se llenan en login directo o step2)
        public int? IdRolSeleccionado { get; set; }
        public int? IdSocioSeleccionado { get; set; }
        public string? CodRolSeleccionado { get; set; }
        public string? NombreSocioSeleccionado { get; set; }
        public string? NombreRolSeleccionado { get; set; }
        public string? LogoSocioSeleccionado { get; set; }
    }

    public class LoginStep2RequestDto
    {
        public int IdUser { get; set; }
        public int IdRol { get; set; }
        public int IdSocio { get; set; }
        public string TempToken { get; set; } = string.Empty;
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

    public class ChangePasswordDto
    {
        public int UserId { get; set; }
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ForgotPasswordDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class VerifyEmailDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ConfirmEmailDto
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class OperationResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
