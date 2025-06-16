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
    public class TicketFrenteSubFrenteRepository : ITicketFrenteSubFrenteRepository
    {
        private readonly ApplicationDbContext _context;

        public TicketFrenteSubFrenteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TicketFrenteSubFrente>> GetByTicketIdAsync(int idTicket)
        {
            return await _context.TicketFrenteSubFrente
                .Where(tfsf => tfsf.IdTicket == idTicket)
                .ToListAsync();
        }

        public async Task<IEnumerable<TicketFrenteSubFrente>> GetActivosByTicketIdAsync(int idTicket)
        {
            return await _context.TicketFrenteSubFrente
                .Where(tfsf => tfsf.IdTicket == idTicket && tfsf.Activo)
                .ToListAsync();
        }

        public async Task<TicketFrenteSubFrente> CreateAsync(TicketFrenteSubFrente frenteSubFrente)
        {
            _context.TicketFrenteSubFrente.Add(frenteSubFrente);
            await _context.SaveChangesAsync();
            return frenteSubFrente;
        }

        public async Task<TicketFrenteSubFrente> UpdateAsync(TicketFrenteSubFrente frenteSubFrente)
        {
            _context.TicketFrenteSubFrente.Update(frenteSubFrente);
            await _context.SaveChangesAsync();
            return frenteSubFrente;
        }

        public async Task<bool> DeactivateAllByTicketIdAsync(int idTicket, string usuarioModificacion)
        {
            var frenteSubFrentes = await _context.TicketFrenteSubFrente
                .Where(tfsf => tfsf.IdTicket == idTicket && tfsf.Activo)
                .ToListAsync();

            foreach (var item in frenteSubFrentes)
            {
                item.Activo = false;
                item.FechaModificacion = DateTime.Now;
                item.UsuarioModificacion = usuarioModificacion;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var frenteSubFrente = await _context.TicketFrenteSubFrente.FindAsync(id);
            if (frenteSubFrente == null) return false;

            _context.TicketFrenteSubFrente.Remove(frenteSubFrente);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
