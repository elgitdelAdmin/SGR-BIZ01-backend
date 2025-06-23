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
    public class PaisRepository : IPaisRepository
    {
        private readonly ApplicationDbContext _context;

        public PaisRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Pais>> GetAllAsync()
        {
            return await _context.Pais.ToListAsync();
        }

        public async Task<IEnumerable<Pais>> GetActiveAsync()
        {
            return await _context.Pais.Where(p => p.Activo).ToListAsync();
        }

        public async Task<Pais?> GetByIdAsync(int id)
        {
            return await _context.Pais.FindAsync(id);
        }

        public async Task<Pais?> GetByCodigoAsync(string codigo)
        {
            return await _context.Pais.FirstOrDefaultAsync(p => p.Codigo == codigo);
        }

        public async Task<Pais> CreateAsync(Pais pais)
        {
            _context.Pais.Add(pais);
            await _context.SaveChangesAsync();
            return pais;
        }

        public async Task<Pais> UpdateAsync(Pais pais)
        {
            _context.Pais.Update(pais);
            await _context.SaveChangesAsync();
            return pais;
        }

        public async Task<bool> DeleteAsync(int id, string? usuarioModificacion = null)
        {
            var pais = await _context.Pais.FindAsync(id);
            if (pais == null) return false;

            pais.Activo = false;
            pais.FechaModificacion = DateTime.Now;
            pais.UsuarioModificacion = usuarioModificacion;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Pais.AnyAsync(p => p.Id == id);
        }

        public async Task<bool> ExistsByCodigoAsync(string codigo, int? excludeId = null)
        {
            var query = _context.Pais.Where(p => p.Codigo == codigo);
            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}
