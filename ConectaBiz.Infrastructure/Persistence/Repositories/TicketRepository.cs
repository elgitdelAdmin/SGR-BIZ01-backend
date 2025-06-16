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
    public class TicketRepository : ITicketRepository
    {
        private readonly ApplicationDbContext _context;

        public TicketRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Ticket>> GetAllAsync()
        {
            return await _context.Ticket
                .Include(t => t.ConsultorAsignaciones.Where(ca => ca.Activo))
                .Include(t => t.FrenteSubFrentes.Where(fsf => fsf.Activo))
                .ToListAsync();
        }

        public async Task<Ticket?> GetByIdAsync(int id)
        {
            return await _context.Ticket.FindAsync(id);
        }

        public async Task<Ticket?> GetByIdWithRelationsAsync(int id)
        {
            return await _context.Ticket
                .Include(t => t.ConsultorAsignaciones)
                .Include(t => t.FrenteSubFrentes)
                .Include(t => t.TicketHistorialEstado)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Ticket?> GetByCodTicketAsync(string codTicket)
        {
            return await _context.Ticket
                .Include(t => t.ConsultorAsignaciones.Where(ca => ca.Activo))
                .Include(t => t.FrenteSubFrentes.Where(fsf => fsf.Activo))
                .FirstOrDefaultAsync(t => t.CodTicket == codTicket);
        }

        public async Task<IEnumerable<Ticket>> GetByEmpresaAsync(int idEmpresa)
        {
            return await _context.Ticket
                .Where(t => t.IdEmpresa == idEmpresa)
                .Include(t => t.ConsultorAsignaciones.Where(ca => ca.Activo))
                .Include(t => t.FrenteSubFrentes.Where(fsf => fsf.Activo))
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByEstadoAsync(int idEstado)
        {
            return await _context.Ticket
                .Where(t => t.IdEstadoTicket == idEstado)
                .Include(t => t.ConsultorAsignaciones.Where(ca => ca.Activo))
                .Include(t => t.FrenteSubFrentes.Where(fsf => fsf.Activo))
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByGestorAsync(int idGestor)
        {
            return await _context.Ticket
                .Where(t => t.IdGestorAsignado == idGestor)
                .Include(t => t.ConsultorAsignaciones.Where(ca => ca.Activo))
                .Include(t => t.FrenteSubFrentes.Where(fsf => fsf.Activo))
                .ToListAsync();
        }

        public async Task<Ticket> CreateAsync(Ticket ticket)
        {
            _context.Ticket.Add(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<Ticket> UpdateAsync(Ticket ticket)
        {
            _context.Ticket.Update(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket == null) return false;

            _context.Ticket.Remove(ticket);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string codTicket, int? excludeId = null)
        {
            return await _context.Ticket
                .AnyAsync(t => t.CodTicket == codTicket && (excludeId == null || t.Id != excludeId));
        }

        public async Task<IEnumerable<Ticket>> GetTicketsWithFiltersAsync(int? idEmpresa = null, int? idEstado = null, bool? urgente = null)
        {
            var query = _context.Ticket.AsQueryable();

            if (idEmpresa.HasValue)
                query = query.Where(t => t.IdEmpresa == idEmpresa.Value);

            if (idEstado.HasValue)
                query = query.Where(t => t.IdEstadoTicket == idEstado.Value);

            return await query
                .Include(t => t.ConsultorAsignaciones.Where(ca => ca.Activo))
                .Include(t => t.FrenteSubFrentes.Where(fsf => fsf.Activo))
                .ToListAsync();
        }
        public async Task<IEnumerable<TicketConsultorAsignacion>> GetConsultorAsignacionesActivasByTicketIdAsync(int idTicket)
        {
            return await _context.Set<TicketConsultorAsignacion>()
                .Where(x => x.IdTicket == idTicket && x.Activo)
                .ToListAsync();
        }

        public async Task<IEnumerable<TicketFrenteSubFrente>> GetFrenteSubFrentesActivosByTicketIdAsync(int idTicket)
        {
            return await _context.Set<TicketFrenteSubFrente>()
                .Where(x => x.IdTicket == idTicket && x.Activo)
                .ToListAsync();
        }
    }
}
