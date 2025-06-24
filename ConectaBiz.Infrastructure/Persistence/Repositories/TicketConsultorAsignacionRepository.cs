using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using ConectaBiz.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Infrastructure.Persistence.Repositories
{
    public class TicketConsultorAsignacionRepository : ITicketConsultorAsignacionRepository
    {
        private readonly ApplicationDbContext _context;

        public TicketConsultorAsignacionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TicketConsultorAsignacion>> GetByTicketIdAsync(int idTicket)
        {
            return await _context.TicketConsultorAsignacion
                .Where(tca => tca.IdTicket == idTicket)
                .ToListAsync();
        }

        public async Task<IEnumerable<TicketConsultorAsignacion>> GetActivosByTicketIdAsync(int idTicket)
        {
            return await _context.TicketConsultorAsignacion
                .Where(tca => tca.IdTicket == idTicket && tca.Activo)
                .ToListAsync();
        }

        public async Task<TicketConsultorAsignacion> CreateAsync(TicketConsultorAsignacion asignacion)
        {
            _context.TicketConsultorAsignacion.Add(asignacion);
            await _context.SaveChangesAsync();
            return asignacion;
        }

        public async Task<TicketConsultorAsignacion> UpdateAsync(TicketConsultorAsignacion asignacion)
        {
            _context.TicketConsultorAsignacion.Update(asignacion);
            await _context.SaveChangesAsync();
            return asignacion;
        }

        public async Task<bool> DeactivateAllByTicketIdAsync(int idTicket, string usuarioDesasignacion)
        {
            var asignaciones = await _context.TicketConsultorAsignacion
                .Where(tca => tca.IdTicket == idTicket && tca.Activo)
                .ToListAsync();

            foreach (var asignacion in asignaciones)
            {
                asignacion.Activo = false;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var asignacion = await _context.TicketConsultorAsignacion.FindAsync(id);
            if (asignacion == null) return false;

            // Eliminación lógica
            asignacion.Activo = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
