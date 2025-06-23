using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using ConectaBiz.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ConectaBiz.Infrastructure.Persistence.Repositories
{
    public class ConsultorRepository : IConsultorRepository
    {
        private readonly ApplicationDbContext _context;

        public ConsultorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Consultor>> GetAllAsync()
        {
            return await _context.Consultor
                .Include(c => c.Persona)
                .Include(c => c.Socio)
                .Include(c => c.ConsultorFrenteSubFrente.Where(cf => cf.Activo))
                    .ThenInclude(cf => cf.Frente)
                .Include(c => c.ConsultorFrenteSubFrente.Where(cf => cf.Activo))
                    .ThenInclude(cf => cf.SubFrente)
                .Where(c => c.Activo)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Consultor> GetByIdAsync(int id)
        {
            return await _context.Consultor
                .Include(c => c.Persona)
                .Include(c => c.Socio)
                .Include(c => c.ConsultorFrenteSubFrente.Where(cf => cf.Activo))
                    .ThenInclude(cf => cf.Frente)
                .Include(c => c.ConsultorFrenteSubFrente.Where(cf => cf.Activo))
                    .ThenInclude(cf => cf.SubFrente)
                .FirstOrDefaultAsync(c => c.Id == id && c.Activo);
        }

        public async Task<Consultor> CreateAsync(Consultor consultor)
        {
            _context.Consultor.Add(consultor);
            await _context.SaveChangesAsync();

            // Cargar las relaciones para devolver la entidad completa
            return await GetByIdAsync(consultor.Id);
        }

        public async Task<Consultor> UpdateAsync(Consultor consultor)
        {
            var existingConsultor = await _context.Consultor
                .FirstOrDefaultAsync(c => c.Id == consultor.Id && c.Activo);

            if (existingConsultor == null)
                return null;

            // Actualizar propiedades
            existingConsultor.IdNivelExperiencia = consultor.IdNivelExperiencia;
            existingConsultor.IdModalidadLaboral = consultor.IdModalidadLaboral;
            existingConsultor.FechaActualizacion = DateTime.Now;
            await _context.SaveChangesAsync();

            // Cargar las relaciones para devolver la entidad completa
            return await GetByIdAsync(consultor.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var consultor = await _context.Consultor
                    .Include(c => c.Persona)
                    .FirstOrDefaultAsync(c => c.Id == id && c.Activo);

                if (consultor == null)
                    return false;

                // Marcar como inactivo en lugar de eliminar físicamente
                consultor.Activo = false;
                consultor.FechaActualizacion = DateTime.Now;

                // También marcar la persona como inactiva
                if (consultor.Persona != null)
                {
                    consultor.Persona.Activo = false;
                    consultor.Persona.FechaActualizacion = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Consultor
                .AnyAsync(c => c.Id == id && c.Activo);
        }

        public async Task<bool> ExistsByPersonaIdAsync(int personaId)
        {
            return await _context.Consultor
                .AnyAsync(c => c.PersonaId == personaId && c.Activo);
        }
        public async Task<bool> ExistsByNumeroDocumentoAsync(string numeroDocumento)
        {
            // Esta consulta busca si existe un consultor activo asociado a una persona
            // con el número de documento proporcionado
            return await _context.Consultor
                .Include(c => c.Persona)
                .AnyAsync(c => c.Persona.NumeroDocumento == numeroDocumento &&
                               c.Activo &&
                               c.Persona.Activo);
        }
    }
}
