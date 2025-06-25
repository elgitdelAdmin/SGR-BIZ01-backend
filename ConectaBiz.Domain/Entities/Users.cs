using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Entities
{
    // User y RefreshToken se mantienen en inglés como estaban originalmente
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public int IdSocio { get; set; } // Clave foránea
        public int IdRol { get; set; } // Clave foránea
        public int IdPersona { get; set; } // Clave foránea
        public bool Activo { get; set; }

        // Propiedades de navegación
        public virtual ICollection<PersonaUser> Personas { get; set; } = new List<PersonaUser>();
        public virtual ICollection<UserRol> Roles { get; set; } = new List<UserRol>();
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public Socio Socio { get; set; }
        public Persona Persona { get; set; }
        public Rol Rol { get; set; }
    }

    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; }
        public int UserId { get; set; }

        // Propiedad de navegación
        public virtual User User { get; set; }
    }

    // El resto de las clases en español
    public class Persona
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
        public string UsuarioCreacion { get; set; }
        public string? UsuarioActualizacion { get; set; }
        public bool Activo { get; set; }

        // Propiedades de navegación
        public virtual ICollection<User> Users { get; set; }
        public virtual Consultor? Consultor { get; set; }
    }

    public class PersonaUser
    {
        public int Id { get; set; }
        public int PersonaId { get; set; }
        public int UserId { get; set; }
        public bool EsPrincipal { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Propiedades de navegación
        public virtual Persona Persona { get; set; }
        public virtual User User { get; set; }
    }

    public class Rol
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string UsuarioCreacion { get; set; } = string.Empty;
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }
        public bool Activo { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
    }


    public class UserRol
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RolId { get; set; }
        public DateTime FechaAsignacion { get; set; }

        // Propiedades de navegación
        public virtual User User { get; set; }
        public virtual Rol Rol { get; set; }
    }

    public class Permiso
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Codigo { get; set; }
        public string? Descripcion { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Propiedades de navegación
        public virtual ICollection<RolPermiso> Roles { get; set; } = new List<RolPermiso>();
    }

    public class RolPermiso
    {
        public int Id { get; set; }
        public int RolId { get; set; }
        public int PermisoId { get; set; }
        public DateTime FechaAsignacion { get; set; }

        // Propiedades de navegación
        public virtual Rol Rol { get; set; }
        public virtual Permiso Permiso { get; set; }
    }
}
