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
    public class EmpresaRepository : IEmpresaRepository
    {
        private readonly ApplicationDbContext _context;
        private bool _disposed = false;

        public EmpresaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Empresa>> GetAllAsync()
        {
            return await _context.Empresas
                .Include(e => e.Pais)
                .Include(e => e.Gestor)
                .Include(e => e.PersonaResponsable) 
                .OrderBy(e => e.RazonSocial)
                .ToListAsync();
        }
        public async Task<IEnumerable<Empresa>> GetByIdSocio(int idSocio)
        {
            return await _context.Empresas
                .Where(e => e.Activo && e.IdSocio == idSocio)
                .Include(e => e.Pais)
                .Include(e => e.Gestor)
                .Include(e => e.PersonaResponsable)
                .OrderBy(e => e.RazonSocial)
                .ToListAsync();
        }
        public async Task<IEnumerable<Empresa>> GetByIdGestorCuenta(int idGestorCuenta, int IdSocio)
        {
            return await _context.Empresas
                .Where(e => e.Activo && e.IdGestor == idGestorCuenta && e.IdSocio == IdSocio)
                .Include(e => e.Pais)
                .Include(e => e.Gestor)
                .Include(e => e.PersonaResponsable)
                .OrderBy(e => e.RazonSocial)
                .ToListAsync();
        }
        public async Task<IEnumerable<Empresa>> GetAllActiveAsync()
        {
            return await _context.Empresas
                .Where(e => e.Activo)
                .Include(e => e.Pais)
                .Include(e => e.Gestor)
                .ThenInclude(g => g.Persona)
                //.Include(e => e.Socio)
                .OrderBy(e => e.RazonSocial)
                .ToListAsync();
        }

        public async Task<Empresa?> GetByIdAsync(int id)
        {
            for (int retry = 0; retry < 3; retry++)
            {
                try
                {
                    return await _context.Empresas
                        .Include(e => e.Pais)
                        .Include(e => e.Gestor)
                        .Include(e => e.PersonaResponsable)
                        .FirstOrDefaultAsync(e => e.Id == id);
                }
                catch (Exception) when (retry < 2)
                {
                    await Task.Delay(1000); // Esperar 1 segundo antes del retry
                }
            }
            return null;
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
                .Include(e => e.Pais)
                .Include(e => e.Gestor)
                    .ThenInclude(g => g.Persona)
                //.Include(e => e.Socio)
                .FirstOrDefaultAsync(e => e.IdUser == iduser);
        }
        public async Task<Empresa?> GetByNumDocContribuyenteAsync(string numDocContribuyente, string numDocSocio)
        {
            return await _context.Empresas
                .Where(e => e.Activo)
                .Include(e => e.Pais)
                .Include(e => e.Gestor)
                    .ThenInclude(g => g.Persona)
                .Include(e => e.Socio)
                .FirstOrDefaultAsync(e => e.NumDocContribuyente == numDocContribuyente
                                          && e.Socio != null
                                          && e.Socio.NumDocContribuyente == numDocSocio);
        }

        public async Task<Empresa?> GetByNumDocContribuyenteDatAsync(string numDocContribuyente)
        {
            return await _context.Empresas
                .Where(e => e.Activo)
                .Include(e => e.Pais)
                .Include(e => e.Gestor)
                    .ThenInclude(g => g.Persona)
                .Include(e => e.Socio)
                .FirstOrDefaultAsync(e => e.NumDocContribuyente == numDocContribuyente);
        }

        public async Task<Empresa?> GetByCodigoAsync(string codigo)
        {
            return await _context.Empresas
                .Include(e => e.Pais)
                .Include(e => e.Gestor)
                    .ThenInclude(g => g.Persona)
                //.Include(e => e.Socio)
                .FirstOrDefaultAsync(e => e.Codigo == codigo);
        }

        public async Task<IEnumerable<Empresa>> GetBySocioAsync(int idSocio)
        {
            return await _context.Empresas
                .Where(e => e.IdSocio == idSocio)
                .Include(e => e.Pais)
                .Include(e => e.Gestor)
                    .ThenInclude(g => g.Persona)
                //.Include(e => e.Socio)
                .OrderBy(e => e.RazonSocial)
                .ToListAsync();
        }

        public async Task<IEnumerable<Empresa>> GetByGestorAsync(int idGestor)
        {
            return await _context.Empresas
                .Where(e => e.IdGestor == idGestor)
                .Include(e => e.Pais)
                .Include(e => e.Gestor)
                    .ThenInclude(g => g.Persona)
                //.Include(e => e.Socio)
                .OrderBy(e => e.RazonSocial)
                .ToListAsync();
        }

        public async Task<Empresa> CreateAsync(Empresa empresa)
        {
            try
            {
                _context.Empresas.Add(empresa);
                await _context.SaveChangesAsync();
                return empresa;
            }
            catch (Exception ex)
            {

                throw;
            }
       
        }

        public async Task<Empresa> UpdateAsync(Empresa empresa)
        {
            _context.Entry(empresa).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return empresa;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null)
                return false;

            // Eliminación lógica
            empresa.Activo = false;
            empresa.FechaModificacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsByNumDocYPaisAsync(string numDocContribuyente, int? idPais)
        {
            return await _context.Empresas.AnyAsync(e =>
                e.NumDocContribuyente == numDocContribuyente &&
                e.IdPais == idPais);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Empresas.AnyAsync(e => e.Id == id);
        }

        public async Task<Empresa?> GetByCodSgrCstiAsync(int codSgrCsti)
        {
            return await _context.Empresas
                .FirstOrDefaultAsync(e => e.CodSgrCsti == codSgrCsti);
        }

        // Implementación de Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose(); // Libera el DbContext
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
