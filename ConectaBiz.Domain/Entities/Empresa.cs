using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ConectaBiz.Domain.Entities
{
    [Table("Empresa")]
    public class Empresa
    {
        // Propiedades de solo-lectura protegidas
        public int Id { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public DateTime FechaRegistro { get; private set; }
        public string? UsuarioRegistro { get; private set; }

        // Propiedades editables por comportamientos
        public string RazonSocial { get; private set; } = string.Empty;
        public string? NombreComercial { get; private set; }
        public string? NumDocContribuyente { get; private set; }
        public string? Direccion { get; private set; }
        public string? Telefono { get; private set; }
        public string? Email { get; private set; }
        public string? CargoResponsable { get; private set; }
        public bool Activo { get; private set; } = true;
        public DateTime? FechaModificacion { get; private set; }
        public string? UsuarioModificacion { get; private set; }
        public int? IdPais { get; private set; }
        public int? IdGestor { get; private set; }
        public int IdSocio { get; private set; }
        public int IdPersonaResponsable { get; private set; }
        public int? CodSgrCsti { get; private set; }
        public int? IdUser { get; private set; }

        // Navegación
        public virtual Pais? Pais { get; private set; }
        public virtual Gestor? Gestor { get; private set; }
        public virtual Socio? Socio { get; private set; }
        public virtual Persona? PersonaResponsable { get; private set; }
        public virtual ICollection<EmpresaGestor> EmpresaGestores { get; private set; } = new List<EmpresaGestor>();

        // Constructor vacío REQUERIDO por EF Core
        protected Empresa() { }

        // Factory Method (creación)
        public static Empresa Crear(
            string razonSocial,
            string? nombreComercial,
            string? numDocContribuyente,
            int idSocio,
            int? idPais,
            int idPersonaResponsable,
            int? idUser,
            string? cargoResponsable,
            string? usuarioRegistro,
            int? codSgrCsti = null)
        {
            if (string.IsNullOrWhiteSpace(razonSocial))
                throw new ArgumentException("La razón social es obligatoria.", nameof(razonSocial));
            if (idSocio <= 0)
                throw new ArgumentException("Se debe especificar un socio válido.", nameof(idSocio));

            return new Empresa
            {
                RazonSocial = razonSocial,
                NombreComercial = nombreComercial,
                NumDocContribuyente = numDocContribuyente,
                IdSocio = idSocio,
                IdPais = idPais,
                IdPersonaResponsable = idPersonaResponsable,
                IdUser = idUser,
                CargoResponsable = cargoResponsable,
                UsuarioRegistro = usuarioRegistro,
                FechaRegistro = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
                Activo = true,
                CodSgrCsti = codSgrCsti
            };
        }

        // Método especial para materializar desde ADO.NET (repositorios externos)
        public static Empresa Materializar(
            int? codSgrCsti,
            string razonSocial,
            string? nombreComercial,
            string? numDocContribuyente,
            string? direccion,
            string? telefono,
            bool activo)
        {
            return new Empresa
            {
                CodSgrCsti = codSgrCsti,
                RazonSocial = razonSocial,
                NombreComercial = nombreComercial,
                NumDocContribuyente = numDocContribuyente,
                Direccion = direccion,
                Telefono = telefono,
                Activo = activo
            };
        }

        // Comportamiento: Actualizar datos generales
        public void Actualizar(
            string razonSocial,
            string? nombreComercial,
            string? numDocContribuyente,
            string? direccion,
            string? telefono,
            string? email,
            string? cargoResponsable,
            bool activo,
            int? idPais,
            int idPersonaResponsable,
            int? idUser,
            string? usuarioModificacion)
        {
            if (string.IsNullOrWhiteSpace(razonSocial))
                throw new ArgumentException("La razón social no puede estar vacía.", nameof(razonSocial));

            RazonSocial = razonSocial;
            NombreComercial = nombreComercial;
            NumDocContribuyente = numDocContribuyente;
            Direccion = direccion;
            Telefono = telefono;
            Email = email;
            CargoResponsable = cargoResponsable;
            Activo = activo;
            IdPais = idPais;
            IdPersonaResponsable = idPersonaResponsable;
            IdUser = idUser;
            UsuarioModificacion = usuarioModificacion;
            FechaModificacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
        }

        // Comportamiento: Asignar responsable
        public void AsignarResponsable(int idPersona, int? idUsuarioSistema)
        {
            IdPersonaResponsable = idPersona;
            IdUser = idUsuarioSistema;
        }

        // Comportamiento: Baja lógica
        public void Desactivar(string? usuarioModificacion)
        {
            Activo = false;
            UsuarioModificacion = usuarioModificacion;
            FechaModificacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
        }

        // Comportamiento: Sincronización de Gestores
        public void SincronizarGestores(List<int> nuevosIdsGestores, int? idGestorPrincipal, string usuario)
        {
            nuevosIdsGestores ??= new List<int>();
            var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

            if (idGestorPrincipal.HasValue && idGestorPrincipal.Value > 0
                && !nuevosIdsGestores.Contains(idGestorPrincipal.Value))
            {
                nuevosIdsGestores.Add(idGestorPrincipal.Value);
            }

            foreach (var eg in EmpresaGestores.Where(e => e.Activo))
            {
                if (!nuevosIdsGestores.Contains(eg.IdGestor))
                {
                    eg.Desasignar(usuario, now);
                }
            }

            foreach (var idG in nuevosIdsGestores)
            {
                bool esPrincipal = idGestorPrincipal.HasValue && idGestorPrincipal.Value == idG;
                var existente = EmpresaGestores.FirstOrDefault(e => e.IdGestor == idG);

                if (existente != null)
                {
                    existente.Reactivar(esPrincipal, usuario, now);
                }
                else
                {
                    EmpresaGestores.Add(EmpresaGestor.Crear(Id, idG, esPrincipal, usuario, now));
                }
            }

            IdGestor = idGestorPrincipal;
        }
    }
}
