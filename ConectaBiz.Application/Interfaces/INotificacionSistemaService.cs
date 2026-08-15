using System.Threading.Tasks;
using ConectaBiz.Application.DTOs;

namespace ConectaBiz.Application.Interfaces
{
    public interface INotificacionSistemaService
    {
        Task EnviarAsync(NotificacionSistemaDto request);
        Task EnviarLoteAsync(System.Collections.Generic.IEnumerable<NotificacionSistemaDto> requests);
        Task<System.Collections.Generic.IEnumerable<ConectaBiz.Domain.Entities.NotificacionSistema>> ObtenerTodasPorUsuarioAsync(int idUser);
        Task<System.Collections.Generic.IEnumerable<ConectaBiz.Domain.Entities.NotificacionSistema>> ObtenerPorReferenciaYUsuariosAsync(int idReferencia, int[] idUsers);
        Task<System.Collections.Generic.IEnumerable<ConectaBiz.Domain.Entities.NotificacionSistema>> ObtenerNoLeidasPorUsuarioAsync(int idUser);
        Task MarcarComoLeidaAsync(int idUser, System.Collections.Generic.List<int> idsNotificaciones);
        Task EliminarNotificacionAsync(int id);
    }
}
