using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Interfaces
{
    public interface INotificacionTicketRepository
    {
        Task<IEnumerable<NotificacionTicket>> GetNotificacionesByUserIdAsync(int idUser);
        Task<IEnumerable<NotificacionTicket>> GetNotificacionesNoLeidasByUserIdAsync(int idUser);
        Task<IEnumerable<NotificacionTicket>> GetNotificacionesByIdTicketAsync(int idTicket);
        Task<IEnumerable<NotificacionTicket>> GetNotificacionesByIdTicketIdUserAsync(int idTicket, int idUser);
        Task<IEnumerable<NotificacionTicket>> GetNotificacionesByIdTicketIdUsersAsync(int idTicket, int[] idUsers);
        Task<NotificacionTicket> GetByIdAsync(int id);
        Task<NotificacionTicket> AddAsync(NotificacionTicket notificacion);
        Task<IEnumerable<NotificacionTicket>> AddRangeAsync(IEnumerable<NotificacionTicket> notificaciones);
        Task UpdateAsync(NotificacionTicket notificacion);
        Task<bool> MarcarComoLeidaAsync(int idUser, int[] idsNotificaciones);
        //Task<bool> MarcarTodasComoLeidasAsync(int idUser);
        Task<bool> DesactivarAsync(int idUser, int[] idsNotificaciones);
        Task<int> ContarNoLeidasAsync(int idUser);
    }
}
