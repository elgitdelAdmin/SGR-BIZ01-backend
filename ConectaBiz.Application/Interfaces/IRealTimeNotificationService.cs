using System.Collections.Generic;
using System.Threading.Tasks;
using ConectaBiz.Domain.Entities;

namespace ConectaBiz.Application.Interfaces
{
    public interface IRealTimeNotificationService
    {
        Task SendNotificationToUserAsync(int idUser, NotificacionSistema notificacion);
        Task SendNotificationsToUsersAsync(IEnumerable<NotificacionSistema> notificaciones);
    }
}
