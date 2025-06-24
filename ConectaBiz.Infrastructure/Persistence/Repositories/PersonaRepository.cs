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
    public class PersonaRepository : IPersonaRepository
    {
        private readonly ApplicationDbContext _context;

        public PersonaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Persona>> GetAllAsync()
        {
            return await _context.Persona
                .Where(p => p.Activo)
                .ToListAsync();
        }

        public async Task<Persona> GetByIdAsync(int id)
        {
            return await _context.Persona
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<Persona> GetByNumeroDocumentoAsync(string numeroDocumento)
        {
            return await _context.Persona
                .FirstOrDefaultAsync(p => p.NumeroDocumento == numeroDocumento && p.Activo);
        }
        public async Task<Persona> CreateAsync(Persona persona)
        {
            persona.FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            persona.Activo = true;

            _context.Persona.Add(persona);
            await _context.SaveChangesAsync();
            return persona;
        }

        public async Task<Persona> UpdateAsync(Persona persona)
        {
            var existingPersona = await _context.Persona.FindAsync(persona.Id);

            if (existingPersona == null)
                return null;

            // Actualizar propiedades
            existingPersona.Nombres = persona.Nombres;
            existingPersona.ApellidoPaterno = persona.ApellidoPaterno;
            existingPersona.ApellidoMaterno = persona.ApellidoMaterno;
            existingPersona.NumeroDocumento = persona.NumeroDocumento;
            existingPersona.TipoDocumento = persona.TipoDocumento;
            existingPersona.Telefono = persona.Telefono;
            existingPersona.Direccion = persona.Direccion;
            existingPersona.FechaNacimiento = persona.FechaNacimiento;
            existingPersona.FechaActualizacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            existingPersona.Activo = persona.Activo;

            await _context.SaveChangesAsync();
            return existingPersona;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // Implementar eliminación en cascada
            var persona = await _context.Persona
                .Include(p => p.Consultor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (persona == null)
                return false;

            // Eliminación lógica
            persona.Activo = false;
            persona.FechaActualizacion = DateTime.Now;

            // Si existe un consultor asociado, también se marca como inactivo
            if (persona.Consultor != null)
            {
                persona.Consultor.Activo = false;
                persona.Consultor.FechaActualizacion = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Persona.AnyAsync(p => p.Id == id);
        }

        public async Task<Persona?> GetByTipoNumDocumentoAsync(int tipoDocumento, string numeroDocumento)
        {
            return await _context.Persona
                .FirstOrDefaultAsync(p => p.TipoDocumento == tipoDocumento &&
                                         p.NumeroDocumento == numeroDocumento);
        }
        public async Task<bool> ExistsByTipoNumDocumentoAsync(int tipoDocumento, string numeroDocumento, int? excludeId = null)
        {
            var query = _context.Persona.Where(p => p.TipoDocumento == tipoDocumento &&
                                                    p.NumeroDocumento == numeroDocumento);
            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}
