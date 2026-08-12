using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConectaBiz.Domain.Entities
{
    [Table("EmpresaGestorTipoTicket", Schema = "conectabiz")]
    public class EmpresaGestorTipoTicket
    {
        public int Id { get; private set; }
        public int IdEmpresaGestor { get; private set; }
        public int IdTipoTicket { get; private set; }
        public bool Activo { get; private set; }
        public DateTime FechaCreacion { get; private set; }
        public DateTime? FechaModificacion { get; private set; }
        public string? UsuarioCreacion { get; private set; }
        public string? UsuarioModificacion { get; private set; }

        [ForeignKey("IdEmpresaGestor")]
        public virtual EmpresaGestor EmpresaGestor { get; private set; } = null!;

        protected EmpresaGestorTipoTicket() { } // Para EF Core

        public static EmpresaGestorTipoTicket Crear(int idTipoTicket, string usuario, DateTime now)
        {
            return new EmpresaGestorTipoTicket
            {
                IdTipoTicket = idTipoTicket,
                Activo = true,
                FechaCreacion = now,
                UsuarioCreacion = usuario
            };
        }

        public void Desactivar(string usuario, DateTime now)
        {
            Activo = false;
            FechaModificacion = now;
            UsuarioModificacion = usuario;
        }

        public void Reactivar(string usuario, DateTime now)
        {
            Activo = true;
            FechaModificacion = now;
            UsuarioModificacion = usuario;
        }
    }
}
