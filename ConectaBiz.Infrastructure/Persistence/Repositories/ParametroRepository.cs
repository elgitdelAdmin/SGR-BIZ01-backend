using ConectaBiz.Application.DTOs;
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
    public class ParametroRepository : IParametroRepository
    {
        private readonly ApplicationDbContext _context;

        public ParametroRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Parametro>> GetAllAsync()
        {
            return await _context.Parametros
                .OrderBy(p => p.TipoParametro)
                .ThenBy(p => p.Orden)
                .ToListAsync();
        }

        public async Task<Parametro?> GetByIdAsync(int id)
        {
            return await _context.Parametros.FindAsync(id);
        }

        public async Task<IEnumerable<Parametro>> GetByTipoParametroAsync(string tipoParametro)
        {
            return await _context.Parametros
                .Where(p => p.TipoParametro == tipoParametro)
                .OrderBy(p => p.Orden)
                .ToListAsync();
        }

        public async Task<IEnumerable<Parametro>> GetActivosAsync()
        {
            return await _context.Parametros
                .Where(p => p.Activo)
                .OrderBy(p => p.TipoParametro)
                .ThenBy(p => p.Orden)
                .ToListAsync();
        }

        public async Task<Parametro?> GetByCodigoAsync(string tipoParametro, string codigo)
        {
            return await _context.Parametros
                .FirstOrDefaultAsync(p => p.TipoParametro == tipoParametro && p.Codigo == codigo);
        }

        public async Task<Parametro> CreateAsync(Parametro parametro)
        {
            _context.Parametros.Add(parametro);
            await _context.SaveChangesAsync();
            return parametro;
        }

        public async Task<Parametro> UpdateAsync(Parametro parametro)
        {
            _context.Entry(parametro).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return parametro;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var parametro = await _context.Parametros.FindAsync(id);
            if (parametro == null)
                return false;

            _context.Parametros.Remove(parametro);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string tipoParametro, string codigo, int? excludeId = null)
        {
            var query = _context.Parametros
                .Where(p => p.TipoParametro == tipoParametro && p.Codigo == codigo);

            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}