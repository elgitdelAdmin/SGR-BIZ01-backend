using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using ConectaBiz.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ConectaBiz.Infrastructure.Persistence.Repositories
{
    public class ConsultorFrenteSubFrenteRepository : IConsultorFrenteSubFrenteRepository
    {
        private readonly ApplicationDbContext _context;

        public ConsultorFrenteSubFrenteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ConsultorFrenteSubFrente>> GetByConsultorIdAsync(int consultorId)
        {
            return await _context.ConsultorFrenteSubFrente
                .Include(c => c.Frente)
                .Include(c => c.SubFrente)
                .Where(c => c.ConsultorId == consultorId && c.Activo)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ConsultorFrenteSubFrente> GetByIdAsync(int id)
        {
            return await _context.ConsultorFrenteSubFrente
                .Include(c => c.Frente)
                .Include(c => c.SubFrente)
                .Include(c => c.Consultor)
                .FirstOrDefaultAsync(c => c.Id == id && c.Activo);
        }

        public async Task<ConsultorFrenteSubFrente> CreateAsync(ConsultorFrenteSubFrente consultorFrenteSubFrente)
        {
            consultorFrenteSubFrente.FechaCreacion = DateTime.Now;
            consultorFrenteSubFrente.Activo = true;

            _context.ConsultorFrenteSubFrente.Add(consultorFrenteSubFrente);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(consultorFrenteSubFrente.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _context.ConsultorFrenteSubFrente
                .FirstOrDefaultAsync(c => c.Id == id && c.Activo);

            if (item == null)
                return false;

            // Eliminación lógica
            item.Activo = false;
            item.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByConsultorIdAsync(int consultorId)
        {
            var items = await _context.ConsultorFrenteSubFrente
                .Where(c => c.ConsultorId == consultorId && c.Activo)
                .ToListAsync();

            if (!items.Any())
                return false;

            // Eliminación lógica de todos los elementos
            foreach (var item in items)
            {
                item.Activo = false;
                item.FechaActualizacion = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int consultorId, int frenteId, int subFrenteId)
        {
            return await _context.ConsultorFrenteSubFrente
                .AnyAsync(c => c.ConsultorId == consultorId &&
                              c.IdFrente == frenteId &&
                              c.IdSubFrente == subFrenteId &&
                              c.Activo);
        }
    }
}
