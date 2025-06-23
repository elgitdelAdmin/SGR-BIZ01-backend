using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using ConectaBiz.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Infrastructure.Persistence.Repositories
{
    public class TicketHistorialRepository : ITicketHistorialRepository
    {
        private readonly ApplicationDbContext _context;

        public TicketHistorialRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TicketHistorialEstado>> GetByTicketIdAsync(int idTicket)
        {
            return await _context.TicketHistorialEstado
                .Where(th => th.IdTicket == idTicket)
                .OrderBy(th => th.FechaCambio)
                .ToListAsync();
        }

        public async Task<TicketHistorialEstado> CreateAsync(TicketHistorialEstado historial)
        {
            _context.TicketHistorialEstado.Add(historial);
            await _context.SaveChangesAsync();
            return historial;
        }

        public async Task<TicketHistorialEstado?> GetLastByTicketIdAsync(int idTicket)
        {
            return await _context.TicketHistorialEstado
                .Where(th => th.IdTicket == idTicket)
                .OrderByDescending(th => th.FechaCambio)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var historial = await _context.TicketHistorialEstado.FindAsync(id);
            if (historial == null) return false;

            _context.TicketHistorialEstado.Remove(historial);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
