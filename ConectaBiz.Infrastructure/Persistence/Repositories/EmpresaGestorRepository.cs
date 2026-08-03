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
    public class EmpresaGestorRepository : IEmpresaGestorRepository
    {
        private readonly ApplicationDbContext _context;

        public EmpresaGestorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EmpresaGestor>> GetByEmpresaIdAsync(int idEmpresa)
        {
            return await _context.EmpresaGestores
                .Where(eg => eg.IdEmpresa == idEmpresa)
                .Include(eg => eg.Gestor)
                    .ThenInclude(g => g.Persona)
                .OrderByDescending(eg => eg.Activo)
                .ThenByDescending(eg => eg.EsPrincipal)
                .ThenBy(eg => eg.FechaAsignacion)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmpresaGestor>> GetActiveByEmpresaIdAsync(int idEmpresa)
        {
            return await _context.EmpresaGestores
                .Where(eg => eg.IdEmpresa == idEmpresa && eg.Activo)
                .Include(eg => eg.Gestor)
                    .ThenInclude(g => g.Persona)
                .OrderByDescending(eg => eg.EsPrincipal)
                .ThenBy(eg => eg.FechaAsignacion)
                .ToListAsync();
        }

        public async Task<EmpresaGestor?> GetPrincipalByEmpresaIdAsync(int idEmpresa)
        {
            return await _context.EmpresaGestores
                .Include(eg => eg.Gestor)
                    .ThenInclude(g => g.Persona)
                .FirstOrDefaultAsync(eg => eg.IdEmpresa == idEmpresa && eg.Activo && eg.EsPrincipal);
        }

        public async Task<EmpresaGestor> AsociarGestorAsync(int idEmpresa, int idGestor, bool esPrincipal, string usuario)
        {
            var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

            // Si es principal, desmarcar otros principales primero
            if (esPrincipal)
            {
                var principalesActuales = await _context.EmpresaGestores
                    .Where(eg => eg.IdEmpresa == idEmpresa && eg.Activo && eg.EsPrincipal && eg.IdGestor != idGestor)
                    .ToListAsync();

                foreach (var p in principalesActuales)
                {
                    p.CambiarPrincipal(false, usuario, now);
                }
            }

            var existente = await _context.EmpresaGestores
                .FirstOrDefaultAsync(eg => eg.IdEmpresa == idEmpresa && eg.IdGestor == idGestor);

            if (existente != null)
            {
                existente.Reactivar(esPrincipal, usuario, now);
                await _context.SaveChangesAsync();
                return existente;
            }

            var nuevo = EmpresaGestor.Crear(idEmpresa, idGestor, esPrincipal, usuario, now);

            _context.EmpresaGestores.Add(nuevo);
            await _context.SaveChangesAsync();
            return nuevo;
        }

        public async Task<bool> DesasociarGestorAsync(int idEmpresa, int idGestor, string usuario)
        {
            var existente = await _context.EmpresaGestores
                .FirstOrDefaultAsync(eg => eg.IdEmpresa == idEmpresa && eg.IdGestor == idGestor && eg.Activo);

            if (existente == null) return false;

            var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            existente.Desasignar(usuario, now);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EstablecerPrincipalAsync(int idEmpresa, int idGestor, string usuario)
        {
            var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            var todosActivos = await _context.EmpresaGestores
                .Where(eg => eg.IdEmpresa == idEmpresa && eg.Activo)
                .ToListAsync();

            var objetivo = todosActivos.FirstOrDefault(eg => eg.IdGestor == idGestor);
            if (objetivo == null)
            {
                // Si no está asociado activamente, lo asociamos como principal
                await AsociarGestorAsync(idEmpresa, idGestor, true, usuario);
                return true;
            }

            foreach (var item in todosActivos)
            {
                bool debeSerPrincipal = (item.IdGestor == idGestor);
                if (item.EsPrincipal != debeSerPrincipal)
                {
                    item.CambiarPrincipal(debeSerPrincipal, usuario, now);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

    }
}
