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
            try
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
            catch (Exception ex)
            {
                throw;
            }
     
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
        public async Task<Consultor> GetByIdUserAsync(int iduser)
        {
            return await _context.Consultor
                .Include(c => c.Persona)
                .Include(c => c.Socio)
                .Include(c => c.ConsultorFrenteSubFrente.Where(cf => cf.Activo))
                    .ThenInclude(cf => cf.Frente)
                .Include(c => c.ConsultorFrenteSubFrente.Where(cf => cf.Activo))
                    .ThenInclude(cf => cf.SubFrente)
                .FirstOrDefaultAsync(c => c.IdUser == iduser && c.Activo);
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
            existingConsultor.FechaActualizacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            existingConsultor.UsuarioActualizacion = consultor.UsuarioActualizacion;
            await _context.SaveChangesAsync();

            // Cargar las relaciones para devolver la entidad completa
            return await GetByIdAsync(consultor.Id);
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var consultor = await _context.Consultor
                            .Include(c => c.Persona)
                            .FirstOrDefaultAsync(c => c.Id == id && c.Activo);
            if (consultor == null) return false;

            consultor.Activo = false;
            consultor.FechaActualizacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            await _context.SaveChangesAsync();
            return true;
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
