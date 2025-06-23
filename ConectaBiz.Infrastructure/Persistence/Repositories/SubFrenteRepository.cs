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
    public class SubFrenteRepository : ISubFrenteRepository
    {
        private readonly ApplicationDbContext _context;

        public SubFrenteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SubFrente>> GetAllAsync()
        {
            return await _context.Set<SubFrente>()
                .Include(sf => sf.Frente)
                .OrderBy(sf => sf.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<SubFrente>> GetActiveAsync()
        {
            return await _context.Set<SubFrente>()
                .Include(sf => sf.Frente)
                .Where(sf => sf.Activo)
                .OrderBy(sf => sf.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<SubFrente>> GetByFrenteIdAsync(int frenteId)
        {
            return await _context.Set<SubFrente>()
                .Include(sf => sf.Frente)
                .Where(sf => sf.IdFrente == frenteId)
                .OrderBy(sf => sf.Nombre)
                .ToListAsync();
        }

        public async Task<SubFrente?> GetByIdAsync(int id)
        {
            return await _context.Set<SubFrente>()
                .FirstOrDefaultAsync(sf => sf.Id == id);
        }

        public async Task<SubFrente?> GetByIdWithFrenteAsync(int id)
        {
            return await _context.Set<SubFrente>()
                .Include(sf => sf.Frente)
                .FirstOrDefaultAsync(sf => sf.Id == id);
        }

        public async Task<SubFrente?> GetByCodigoAsync(string codigo)
        {
            return await _context.Set<SubFrente>()
                .FirstOrDefaultAsync(sf => sf.Codigo == codigo);
        }

        public async Task<SubFrente> CreateAsync(SubFrente subFrente)
        {
            subFrente.FechaRegistro = DateTime.Now;
            _context.Set<SubFrente>().Add(subFrente);
            await _context.SaveChangesAsync();
            return subFrente;
        }

        public async Task<SubFrente> UpdateAsync(SubFrente subFrente)
        {
            subFrente.FechaModificacion = DateTime.Now;
            _context.Set<SubFrente>().Update(subFrente);
            await _context.SaveChangesAsync();
            return subFrente;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var subFrente = await GetByIdAsync(id);
            if (subFrente == null) return false;

            _context.Set<SubFrente>().Remove(subFrente);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Set<SubFrente>().AnyAsync(sf => sf.Id == id);
        }

        public async Task<bool> ExistsByCodigoAsync(string codigo, int? excludeId = null)
        {
            var query = _context.Set<SubFrente>().Where(sf => sf.Codigo == codigo);
            if (excludeId.HasValue)
                query = query.Where(sf => sf.Id != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}
