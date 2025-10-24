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
        public async Task<IEnumerable<Gestor>> GetByIdSocio(int idSocio)
        {
            return await _context.Gestores
                .Include(g => g.Persona)
                .Include(g => g.Socio)
                .Include(g => g.GestorFrenteSubFrente.Where(gf => gf.Activo))
                .Where(g => g.Activo && g.IdSocio == idSocio)
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
        public async Task<IEnumerable<Gestor>> GetByIdsAsync(int[] ids)
        {
            return await _context.Gestores
                .Where(g => ids.Contains(g.Id))                
                .Include(g => g.Persona)
                .Include(g => g.GestorFrenteSubFrente
                               .Where(gf => gf.Activo))        
                .AsNoTracking()                                
                .ToListAsync();                                
        }


        public async Task<IEnumerable<Gestor>> GetByIdRolAsync(int idRol)
        {
            return await _context.Gestores
                .Include(g => g.Persona)
                .Include(g => g.GestorFrenteSubFrente.Where(gf => gf.Activo))
                .Where(g => g.Persona.Users.Any(u => u.IdRol == idRol && u.Activo))
                .ToListAsync();
        }
        public async Task<Gestor?> GetByIdPersonaAsync(int idPersona)
        {
            return await _context.Gestores
                .Include(g => g.Persona)
                .Include(g => g.GestorFrenteSubFrente.Where(gf => gf.Activo))
                .FirstOrDefaultAsync(g => g.PersonaId == idPersona);
        }
        public async Task<Gestor?> GetByIdUserAsync(int iduser)
        {
            return await _context.Gestores
                .Include(g => g.Persona)
                .Include(g => g.GestorFrenteSubFrente.Where(gf => gf.Activo))
                .FirstOrDefaultAsync(g => g.IdUser == iduser);
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
            try
            {
                _context.Entry(gestor).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return gestor;
            }
            catch (Exception ex)
            {

                throw;
            }
           
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
