using System.Threading.Tasks;
using ConectaBiz.Domain.Entities;

namespace ConectaBiz.Domain.Interfaces
{
    public interface INotificacionSistemaRepository
    {
        Task<NotificacionSistema> AddAsync(NotificacionSistema notificacion);
        Task AddRangeAsync(System.Collections.Generic.IEnumerable<NotificacionSistema> notificaciones);
        Task<IEnumerable<NotificacionSistema>> GetNotificacionesByUserIdAsync(int idUser);
        Task<IEnumerable<NotificacionSistema>> GetByReferenciaAndUsersAsync(int idReferencia, int[] idUsers);
        Task<IEnumerable<NotificacionSistema>> GetNotificacionesNoLeidasByUserIdAsync(int idUser);
        Task<NotificacionSistema> GetByIdAsync(int id);
        Task UpdateAsync(NotificacionSistema notificacion);
        Task UpdateRangeAsync(IEnumerable<NotificacionSistema> notificaciones);
        Task MarcarComoLeidaAsync(int idUser, System.Collections.Generic.List<int> idsNotificaciones);
        Task EliminarLogicaAsync(int id);
    }
}
