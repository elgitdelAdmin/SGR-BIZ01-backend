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
    public class FrenteRepository : IFrenteRepository
    {
        private readonly ApplicationDbContext _context;

        public FrenteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Frente>> GetAllAsync()
        {
            return await _context.Set<Frente>()
                .Include(f => f.SubFrente)
                .OrderBy(f => f.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Frente>> GetActiveAsync()
        {
            return await _context.Set<Frente>()
                .Include(f => f.SubFrente.Where(sf => sf.Activo))
                .Where(f => f.Activo)
                .OrderBy(f => f.Nombre)
                .ToListAsync();
        }

        public async Task<Frente?> GetByIdAsync(int id)
        {
            return await _context.Set<Frente>()
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Frente?> GetByIdWithSubFrentesAsync(int id)
        {
            return await _context.Set<Frente>()
                .Include(f => f.SubFrente)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Frente?> GetByCodigoAsync(string codigo)
        {
            return await _context.Set<Frente>()
                .FirstOrDefaultAsync(f => f.Codigo == codigo);
        }

        public async Task<Frente> CreateAsync(Frente frente)
        {
            frente.FechaRegistro = DateTime.Now;
            _context.Set<Frente>().Add(frente);
            await _context.SaveChangesAsync();
            return frente;
        }

        public async Task<Frente> UpdateAsync(Frente frente)
        {
            frente.FechaModificacion = DateTime.Now;
            _context.Set<Frente>().Update(frente);
            await _context.SaveChangesAsync();
            return frente;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var frente = await GetByIdAsync(id);
            if (frente == null) return false;

            // Eliminación lógica
            frente.Activo = false;
            frente.FechaModificacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Set<Frente>().AnyAsync(f => f.Id == id);
        }

        public async Task<bool> ExistsByCodigoAsync(string codigo, int? excludeId = null)
        {
            var query = _context.Set<Frente>().Where(f => f.Codigo == codigo);
            if (excludeId.HasValue)
                query = query.Where(f => f.Id != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}
