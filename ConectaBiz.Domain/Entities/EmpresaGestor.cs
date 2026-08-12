using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConectaBiz.Domain.Entities
{
    [Table("EmpresaGestor", Schema = "conectabiz")]
    public class EmpresaGestor
    {
        public int Id { get; private set; }
        public int IdEmpresa { get; private set; }
        public int IdGestor { get; private set; }
        public bool EsPrincipal { get; private set; }
        public bool Activo { get; private set; }
        public DateTime FechaAsignacion { get; private set; }
        public DateTime? FechaDesasignacion { get; private set; }
        public DateTime FechaCreacion { get; private set; }
        public DateTime? FechaModificacion { get; private set; }
        public string? UsuarioCreacion { get; private set; }
        public string? UsuarioModificacion { get; private set; }

        public virtual Empresa Empresa { get; private set; } = null!;
        public virtual Gestor Gestor { get; private set; } = null!;
        public virtual ICollection<EmpresaGestorTipoTicket> TiposTicketPermitidos { get; private set; } = new List<EmpresaGestorTipoTicket>();

        protected EmpresaGestor() { } // Para EF Core

        // Factory Method
        public static EmpresaGestor Crear(int idEmpresa, int idGestor, bool esPrincipal, string usuario, DateTime now)
        {
            return new EmpresaGestor
            {
                IdEmpresa = idEmpresa,
                IdGestor = idGestor,
                EsPrincipal = esPrincipal,
                Activo = true,
                FechaAsignacion = now,
                FechaCreacion = now,
                UsuarioCreacion = usuario
            };
        }

        // Comportamiento: Desasignar gestor
        public void Desasignar(string usuario, DateTime now)
        {
            Activo = false;
            EsPrincipal = false;
            FechaDesasignacion = now;
            FechaModificacion = now;
            UsuarioModificacion = usuario;
        }

        // Comportamiento: Reactivar gestor
        public void Reactivar(bool esPrincipal, string usuario, DateTime now)
        {
            Activo = true;
            EsPrincipal = esPrincipal;
            FechaAsignacion = now;
            FechaDesasignacion = null;
            FechaModificacion = now;
            UsuarioModificacion = usuario;
        }

        // Comportamiento: Cambiar principal
        public void CambiarPrincipal(bool esPrincipal, string usuario, DateTime now)
        {
            EsPrincipal = esPrincipal;
            FechaModificacion = now;
            UsuarioModificacion = usuario;
        }

        // Comportamiento: Sincronizar Tipos de Ticket permitidos
        public void SincronizarTiposTicket(List<int> idsTiposTicket, string usuario)
        {
            var now = DateTime.Now;
            var tiposActivos = TiposTicketPermitidos.Where(t => t.Activo).ToList();

            // 1. Desactivar los que ya no están en la nueva lista
            foreach (var tipoExistente in tiposActivos)
            {
                if (!idsTiposTicket.Contains(tipoExistente.IdTipoTicket))
                {
                    tipoExistente.Desactivar(usuario, now);
                }
            }

            // 2. Agregar o reactivar los nuevos
            foreach (var idTipo in idsTiposTicket)
            {
                var tipoExistente = TiposTicketPermitidos.FirstOrDefault(t => t.IdTipoTicket == idTipo);
                if (tipoExistente == null)
                {
                    TiposTicketPermitidos.Add(EmpresaGestorTipoTicket.Crear(idTipo, usuario, now));
                }
                else if (!tipoExistente.Activo)
                {
                    tipoExistente.Reactivar(usuario, now);
                }
            }
        }
    }
}
