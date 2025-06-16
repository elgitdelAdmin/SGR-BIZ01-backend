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
    public class GestorFrenteSubFrenteRepository : IGestorFrenteSubFrenteRepository
    {
        private readonly ApplicationDbContext _context;

        public GestorFrenteSubFrenteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GestorFrenteSubFrente>> GetByGestorIdAsync(int IdGestor)
        {
            return await _context.GestorFrenteSubFrente
                .Where(gf => gf.IdGestor == IdGestor && gf.Activo)
                .ToListAsync();
        }

        public async Task<GestorFrenteSubFrente> CreateAsync(GestorFrenteSubFrente gestorFrenteSubFrente)
        {
            _context.GestorFrenteSubFrente.Add(gestorFrenteSubFrente);
            await _context.SaveChangesAsync();
            return gestorFrenteSubFrente;
        }

        public async Task UpdateRangeAsync(IEnumerable<GestorFrenteSubFrente> gestorFrenteSubFrente)
        {
            _context.GestorFrenteSubFrente.UpdateRange(gestorFrenteSubFrente);
            await _context.SaveChangesAsync();
        }

        public async Task DeactivateByGestorIdAsync(int IdGestor)
        {
            var items = await _context.GestorFrenteSubFrente
                .Where(gf => gf.IdGestor == IdGestor && gf.Activo)
                .ToListAsync();

            foreach (var item in items)
            {
                item.Activo = false;
            }

            await _context.SaveChangesAsync();
        }
    }
}
