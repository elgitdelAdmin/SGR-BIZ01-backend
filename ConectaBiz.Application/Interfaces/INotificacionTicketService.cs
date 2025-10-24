using ConectaBiz.Application.DTOs;
using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Interfaces
{
    public interface INotificacionTicketService
    {
        Task<IEnumerable<NotificacionTicketDto>> GetNotificacionesByUserIdAsync(int idUser);
        Task<IEnumerable<NotificacionTicketDto>> GetNotificacionesNoLeidasByUserIdAsync(int idUser);
        Task<IEnumerable<NotificacionTicketDto>> GetNotificacionesByIdTicketAsync(int idTicket);
        Task<IEnumerable<NotificacionTicketDto>> GetNotificacionesByIdTicketIdUserAsync(int idTicket, int idUser);
        Task<IEnumerable<NotificacionTicketDto>> GetNotificacionesByIdTicketIdUsersAsync(int idTicket, int[] idUsers);
        Task<bool> MarcarComoLeidaAsync(int idUser, int[] idsNotificaciones);
        //Task<bool> MarcarTodasComoLeidasAsync(int idUser);
        Task<bool> DesactivarAsync(int idUser, int[] idsNotificaciones);
        Task<NotificacionTicket> AddAsync(CrearNotificacionDto dto);
        Task<IEnumerable<NotificacionTicket>> AddRangeAsync(IEnumerable<CrearNotificacionDto> dtos);
        Task<int> ContarNoLeidasAsync(int idUser);
    }
}
