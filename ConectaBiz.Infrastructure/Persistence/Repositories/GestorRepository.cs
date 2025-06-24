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
    public class GestorRepository : IGestorRepository
    {
        private readonly ApplicationDbContext _context;

        public GestorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Gestor>> GetAllAsync()
        {
            return await _context.Gestores
                .Include(g => g.Persona)
                .Include(g => g.Socio)
                .Include(g => g.GestorFrenteSubFrente.Where(gf => gf.Activo))
                .Where(g => g.Activo)
                .OrderBy(g => g.Persona.Nombres)
                .ThenBy(g => g.Persona.ApellidoPaterno)
                .ToListAsync();
        }

        public async Task<Gestor?> GetByIdAsync(int id)
        {
            return await _context.Gestores
                .Include(g => g.Persona)
                .Include(g => g.GestorFrenteSubFrente.Where(gf => gf.Activo))
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<Gestor?> GetByPersonaIdAsync(int personaId)
        {
            return await _context.Gestores
                .Include(g => g.Persona)
                .Include(g => g.GestorFrenteSubFrente.Where(gf => gf.Activo))
                .FirstOrDefaultAsync(g => g.PersonaId == personaId);
        }

        public async Task<Gestor> CreateAsync(Gestor gestor)
        {
            _context.Gestores.Add(gestor);
            await _context.SaveChangesAsync();
            return gestor;
        }

        public async Task<Gestor> UpdateAsync(Gestor gestor)
        {
            _context.Entry(gestor).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return gestor;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var gestor = await _context.Gestores.FindAsync(id);
            if (gestor == null) return false;

            gestor.Activo = false;
            gestor.FechaActualizacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsByIdAsync(int id)
        {
            return await _context.Gestores.AnyAsync(g => g.Id == id);
        }

        public async Task<bool> ExistsByPersonaIdAsync(int personaId, int? excludeId = null)
        {
            var query = _context.Gestores.Where(g => g.PersonaId == personaId && g.Activo);
            if (excludeId.HasValue)
                query = query.Where(g => g.Id != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}
