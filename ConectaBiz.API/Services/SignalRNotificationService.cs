using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Entities;
using ConectaBiz.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ConectaBiz.API.Services
{
    public class SignalRNotificationService : IRealTimeNotificationService
    {
        private readonly IHubContext<NotificacionHub> _hubContext;

        public SignalRNotificationService(IHubContext<NotificacionHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationToUserAsync(int idUser, NotificacionSistema notificacion)
        {
            // Enviamos al grupo correspondiente al idUser asegurando que las propiedades sean camelCase
            await _hubContext.Clients.Group(idUser.ToString()).SendAsync("RecibirNotificacion", new {
                id = notificacion.Id,
                idUser = notificacion.IdUser,
                tipoNotificacion = notificacion.TipoNotificacion,
                idReferencia = notificacion.IdReferencia,
                rutaFrontend = notificacion.RutaFrontend,
                mensaje = notificacion.Mensaje,
                leido = notificacion.Leido,
                fechaCreacion = notificacion.FechaCreacion,
                activo = notificacion.Activo
            });
        }

        public async Task SendNotificationsToUsersAsync(IEnumerable<NotificacionSistema> notificaciones)
        {
            foreach (var notificacion in notificaciones)
            {
                if (notificacion.IdUser.HasValue)
                {
                    await SendNotificationToUserAsync(notificacion.IdUser.Value, notificacion);
                }
            }
        }
    }
}
