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
    // Repositories/SocioRepository.cs
    public class SocioRepository : ISocioRepository
    {
        private readonly ApplicationDbContext _context;

        public SocioRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Socio>> ListarTodosAsync()
        {
            return await _context.Socios
                .Where(s => s.Activo)
                .OrderBy(s => s.RazonSocial)
                .ToListAsync();
        }

        public async Task<Socio?> ObtenerPorIdAsync(int id)
        {
            return await _context.Socios
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Socio?> ObtenerPorNumDocAsync(string numDoc)
        {
            if (string.IsNullOrWhiteSpace(numDoc))
                return null;

            return await _context.Socios
                .FirstOrDefaultAsync(s => s.NumDocContribuyente == numDoc);
        }

        public async Task<Socio> CrearAsync(Socio socio)
        {
            _context.Socios.Add(socio);
            await _context.SaveChangesAsync();
            return socio;
        }

        public async Task<Socio> ActualizarAsync(Socio socio)
        {
            _context.Socios.Update(socio);
            await _context.SaveChangesAsync();
            return socio;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var socio = await _context.Socios.FindAsync(id);
            if (socio == null)
                return false;

            // Eliminación lógica
            socio.Activo = false;
            socio.FechaModificacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExisteNumDocAsync(string numDoc)
        {
            if (string.IsNullOrWhiteSpace(numDoc))
                return false;

            return await _context.Socios
                .AnyAsync(s => s.NumDocContribuyente == numDoc && s.Activo);
        }

        public async Task<bool> ExisteNumDocAsync(string numDoc, int idExcluir)
        {
            if (string.IsNullOrWhiteSpace(numDoc))
                return false;

            return await _context.Socios
                .AnyAsync(s => s.NumDocContribuyente == numDoc && s.Id != idExcluir && s.Activo);
        }
    }

}
