using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using ConectaBiz.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ConectaBiz.Infrastructure.Persistence.Repositories
{
    public class NotificacionTicketRepository : INotificacionTicketRepository
    {
        private readonly ApplicationDbContext _context;

        public NotificacionTicketRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NotificacionTicket>> GetNotificacionesByUserIdAsync(int idUser)
        {
            return await _context.NotificacionTickets
                .Include(n => n.Ticket)
                .Where(n => n.IdUser == idUser && n.Activo)
                .OrderByDescending(n => n.FechaCreacion)
                .Take(20)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificacionTicket>> GetNotificacionesNoLeidasByUserIdAsync(int idUser)
        {
            return await _context.NotificacionTickets
                .Include(n => n.Ticket)
                .Where(n => n.IdUser == idUser && !n.Leido && n.Activo)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();
        }
        public async Task<IEnumerable<NotificacionTicket>> GetNotificacionesByIdTicketAsync(int idTicket)
        {
            return await _context.NotificacionTickets
                .Include(n => n.Ticket)
                .Where(n => n.IdTicket == idTicket && n.Activo)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();
        }
        public async Task<IEnumerable<NotificacionTicket>> GetNotificacionesByIdTicketIdUserAsync(int idTicket, int idUser)
        {
            return await _context.NotificacionTickets
                .Include(n => n.Ticket)
                .Where(n => n.IdTicket == idTicket && n.IdUser == idUser && n.Activo)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();
        }
        public async Task<IEnumerable<NotificacionTicket>> GetNotificacionesByIdTicketIdUsersAsync(int idTicket, int[] idUsers)
        {
            return await _context.NotificacionTickets
                .Include(n => n.Ticket)
                .Where(n => n.IdTicket == idTicket
                            && idUsers.Contains(n.IdUser)
                            && n.Activo)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();
        }
        public async Task<NotificacionTicket> GetByIdAsync(int id)
        {
            return await _context.NotificacionTickets
                .Include(n => n.Ticket)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<NotificacionTicket> AddAsync(NotificacionTicket notificacion)
        {
            _context.NotificacionTickets.Add(notificacion);
            await _context.SaveChangesAsync();
            return notificacion;
        }
        public async Task<IEnumerable<NotificacionTicket>> AddRangeAsync(IEnumerable<NotificacionTicket> notificaciones)
        {
            _context.NotificacionTickets.AddRange(notificaciones);
            await _context.SaveChangesAsync();
            return notificaciones;
        }
        public async Task UpdateAsync(NotificacionTicket notificacion)
        {
            _context.NotificacionTickets.Update(notificacion);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> MarcarComoLeidaAsync(int idUser, int[] idsNotificaciones)
        {
            await _context.NotificacionTickets
                .Where(n => idsNotificaciones.Contains(n.Id) && n.IdUser == idUser && n.Activo)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(n => n.Leido, true)
                    .SetProperty(n => n.FechaLectura, DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local))
                );
            return true;
        }

        public async Task<bool> DesactivarAsync(int idUser, int[] idsNotificaciones)
        {
            await _context.NotificacionTickets
                .Where(n => idsNotificaciones.Contains(n.Id) && n.IdUser == idUser && n.Activo)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(n => n.Activo, false)
                    .SetProperty(n => n.FechaLectura, DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local))
                );

            return true;
        }


        public async Task<bool> DesactivarAsync(int idTicket, int idUser)
        {
            var notificaciones = await _context.NotificacionTickets
                .Where(n => n.IdTicket == idTicket && n.IdUser == idUser && n.Activo)
                .ToListAsync();

            foreach (var notificacion in notificaciones)
            {
                notificacion.Activo = false;
                notificacion.FechaLectura = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> ContarNoLeidasAsync(int idUser)
        {
            return await _context.NotificacionTickets
                .CountAsync(n => n.IdUser == idUser && !n.Leido && n.Activo);
        }
    }
}
