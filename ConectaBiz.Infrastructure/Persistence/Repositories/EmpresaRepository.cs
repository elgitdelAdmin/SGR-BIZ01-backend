using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using ConectaBiz.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConectaBiz.Infrastructure.Persistence.Repositories
{
    public class EmpresaRepository : IEmpresaRepository
    {
        private readonly ApplicationDbContext _context;

        public EmpresaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Empresa>> GetAllAsync()
        {
            return await _context.Empresas
                .AsNoTracking()
                .AsSplitQuery()
                .Include(e => e.Pais)
                .Include(e => e.Gestor)
                .Include(e => e.PersonaResponsable) 
                .Include(e => e.Socio)
                .Include(e => e.EmpresaGestores)
                    .ThenInclude(eg => eg.Gestor)
                        .ThenInclude(g => g.Persona)
                .OrderBy(e => e.RazonSocial)
                .ToListAsync();
        }

        public async Task<IEnumerable<Empresa>> GetByIdSocio(int idSocio)
        {
            return await _context.Empresas
                .AsNoTracking()
                .AsSplitQuery()
                .Where(e => e.Activo && e.IdSocio == idSocio)
                .Include(e => e.Pais)
                .Include(e => e.Gestor)
                .Include(e => e.PersonaResponsable)
                .Include(e => e.Socio)
                .Include(e => e.EmpresaGestores)
                    .ThenInclude(eg => eg.Gestor)
                        .ThenInclude(g => g.Persona)
                .OrderBy(e => e.RazonSocial)
                .ToListAsync();
        }

        public async Task<IEnumerable<Empresa>> GetByIdGestorCuenta(int idGestorCuenta, int IdSocio)
        {
            return await _context.Empresas
                .AsNoTracking()
                .AsSplitQuery()
                .Where(e => e.Activo && e.IdSocio == IdSocio && (e.EmpresaGestores.Any(eg => eg.Activo && eg.IdGestor == idGestorCuenta) || e.IdGestor == idGestorCuenta))
                .Include(e => e.Pais)
                .Include(e => e.Gestor)
                .Include(e => e.PersonaResponsable)
                .Include(e => e.Socio)
                .Include(e => e.EmpresaGestores)
                    .ThenInclude(eg => eg.Gestor)
                        .ThenInclude(g => g.Persona)
                .OrderBy(e => e.RazonSocial)
                .ToListAsync();
        }

        public async Task<IEnumerable<Empresa>> GetAllActiveAsync()
        {
            return await _context.Empresas
                .AsNoTracking()
                .AsSplitQuery()
                .Where(e => e.Activo)
                .Include(e => e.Pais)
                .Include(e => e.Gestor)
                    .ThenInclude(g => g.Persona)
                .Include(e => e.Socio)
                .Include(e => e.EmpresaGestores)
                    .ThenInclude(eg => eg.Gestor)
                        .ThenInclude(g => g.Persona)
                .OrderBy(e => e.RazonSocial)
                .ToListAsync();
        }

        public async Task<Empresa?> GetByIdAsync(int id)
        {
            return await _context.Empresas
                .AsSplitQuery()
                .Include(e => e.Pais)
                .Include(e => e.Gestor)
                .Include(e => e.PersonaResponsable)
                .Include(e => e.Socio)
                .Include(e => e.EmpresaGestores)
                    .ThenInclude(eg => eg.Gestor)
                        .ThenInclude(g => g.Persona)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Empresa?> GetByIdAsync2(int id)
        {
            return await _context.Empresas
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Empresa?> GetByIdUserAsync(int iduser)
        {
            return await _context.Empresas
                .AsNoTracking()
                .AsSplitQuery()
                .Include(e => e.Pais)
                .Include(e => e.Gestor)
                    .ThenInclude(g => g.Persona)
                .Include(e => e.EmpresaGestores)
                    .ThenInclude(eg => eg.Gestor)
                        .ThenInclude(g => g.Persona)
                .FirstOrDefaultAsync(e => e.IdUser == iduser);
        }

        public async Task<Empresa?> GetByNumDocContribuyenteAsync(string numDocContribuyente, string numDocSocio)
        {
            return await _context.Empresas
                .AsNoTracking()
                .AsSplitQuery()
                .Where(e => e.Activo)
                .Include(e => e.Pais)
                .Include(e => e.Gestor)
                    .ThenInclude(g => g.Persona)
                .Include(e => e.Socio)
                .Include(e => e.EmpresaGestores)
                    .ThenInclude(eg => eg.Gestor)
                        .ThenInclude(g => g.Persona)
                .FirstOrDefaultAsync(e => e.NumDocContribuyente == numDocContribuyente
                                          && e.Socio != null
                                          && e.Socio.NumDocContribuyente == numDocSocio);
        }

        public async Task<Empresa?> GetByNumDocContribuyenteDatAsync(string numDocContribuyente)
        {
            return await _context.Empresas
                .AsNoTracking()
                .AsSplitQuery()
                .Where(e => e.Activo)
                .Include(e => e.Pais)
                .Include(e => e.Gestor)
                    .ThenInclude(g => g.Persona)
                .Include(e => e.Socio)
                .Include(e => e.EmpresaGestores)
                    .ThenInclude(eg => eg.Gestor)
                        .ThenInclude(g => g.Persona)
                .FirstOrDefaultAsync(e => e.NumDocContribuyente == numDocContribuyente);
        }

        public async Task<Empresa?> GetByCodigoAsync(string codigo)
        {
            return await _context.Empresas
                .AsNoTracking()
                .AsSplitQuery()
                .Include(e => e.Pais)
                .Include(e => e.Gestor)
                    .ThenInclude(g => g.Persona)
                .Include(e => e.EmpresaGestores)
                    .ThenInclude(eg => eg.Gestor)
                        .ThenInclude(g => g.Persona)
                .FirstOrDefaultAsync(e => e.Codigo == codigo);
        }

        public async Task<IEnumerable<Empresa>> GetBySocioAsync(int idSocio)
        {
            return await _context.Empresas
                .AsNoTracking()
                .AsSplitQuery()
                .Where(e => e.IdSocio == idSocio)
                .Include(e => e.Pais)
                .Include(e => e.Gestor)
                    .ThenInclude(g => g.Persona)
                .Include(e => e.EmpresaGestores)
                    .ThenInclude(eg => eg.Gestor)
                        .ThenInclude(g => g.Persona)
                .OrderBy(e => e.RazonSocial)
                .ToListAsync();
        }

        public async Task<IEnumerable<Empresa>> GetByGestorAsync(int idGestor)
        {
            return await _context.Empresas
                .AsNoTracking()
                .AsSplitQuery()
                .Where(e => e.EmpresaGestores.Any(eg => eg.Activo && eg.IdGestor == idGestor) || e.IdGestor == idGestor)
                .Include(e => e.Pais)
                .Include(e => e.Gestor)
                    .ThenInclude(g => g.Persona)
                .Include(e => e.EmpresaGestores)
                    .ThenInclude(eg => eg.Gestor)
                        .ThenInclude(g => g.Persona)
                .OrderBy(e => e.RazonSocial)
                .ToListAsync();
        }

        public async Task<Empresa> CreateAsync(Empresa empresa)
        {
            _context.Empresas.Add(empresa);
            await _context.SaveChangesAsync();
            return empresa;
        }

        public async Task<Empresa> UpdateAsync(Empresa empresa)
        {
            // EF Core ya tiene trackeada la entidad por el GetByIdAsync,
            // por lo que SaveChangesAsync detectará las propiedades modificadas automáticamente
            await _context.SaveChangesAsync();
            return empresa;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            throw new NotSupportedException("Usa el método Desactivar del dominio y luego UpdateAsync.");
        }

        public async Task<bool> ExistsByNumDocYPaisAsync(string numDocContribuyente, int? idPais, int? idSocio = null)
        {
            return await _context.Empresas.AnyAsync(e =>
                e.NumDocContribuyente == numDocContribuyente &&
                e.IdPais == idPais &&
                (idSocio == null || e.IdSocio == idSocio));
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Empresas.AnyAsync(e => e.Id == id);
        }

        public async Task<Empresa?> GetByCodSgrCstiAsync(int codSgrCsti)
        {
            return await _context.Empresas
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.CodSgrCsti == codSgrCsti);
        }
    }
}
