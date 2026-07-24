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
                    p.EsPrincipal = false;
                    p.FechaModificacion = now;
                    p.UsuarioModificacion = usuario;
                }
            }

            var existente = await _context.EmpresaGestores
                .FirstOrDefaultAsync(eg => eg.IdEmpresa == idEmpresa && eg.IdGestor == idGestor);

            if (existente != null)
            {
                existente.Activo = true;
                existente.EsPrincipal = esPrincipal;
                existente.FechaAsignacion = now;
                existente.FechaDesasignacion = null;
                existente.FechaModificacion = now;
                existente.UsuarioModificacion = usuario;

                await _context.SaveChangesAsync();
                return existente;
            }

            var nuevo = new EmpresaGestor
            {
                IdEmpresa = idEmpresa,
                IdGestor = idGestor,
                EsPrincipal = esPrincipal,
                Activo = true,
                FechaAsignacion = now,
                FechaCreacion = now,
                UsuarioCreacion = usuario
            };

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
            existente.Activo = false;
            existente.EsPrincipal = false;
            existente.FechaDesasignacion = now;
            existente.FechaModificacion = now;
            existente.UsuarioModificacion = usuario;

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
                    item.EsPrincipal = debeSerPrincipal;
                    item.FechaModificacion = now;
                    item.UsuarioModificacion = usuario;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SincronizarGestoresEmpresaAsync(int idEmpresa, List<int> idsGestores, int? idGestorPrincipal, string usuario)
        {
            idsGestores ??= new List<int>();

            if (idGestorPrincipal.HasValue && idGestorPrincipal.Value > 0 && !idsGestores.Contains(idGestorPrincipal.Value))
            {
                idsGestores.Add(idGestorPrincipal.Value);
            }

            var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            var existentes = await _context.EmpresaGestores
                .Where(eg => eg.IdEmpresa == idEmpresa)
                .ToListAsync();

            // Desactivar los que ya no vienen en la lista
            foreach (var ext in existentes.Where(e => e.Activo))
            {
                if (!idsGestores.Contains(ext.IdGestor))
                {
                    ext.Activo = false;
                    ext.EsPrincipal = false;
                    ext.FechaDesasignacion = now;
                    ext.FechaModificacion = now;
                    ext.UsuarioModificacion = usuario;
                }
            }

            // Agregar / actualizar los de la lista
            foreach (var idG in idsGestores)
            {
                bool esPrincipal = idGestorPrincipal.HasValue && idGestorPrincipal.Value == idG;
                var ext = existentes.FirstOrDefault(e => e.IdGestor == idG);

                if (ext != null)
                {
                    ext.Activo = true;
                    ext.EsPrincipal = esPrincipal;
                    if (ext.FechaDesasignacion.HasValue)
                    {
                        ext.FechaAsignacion = now;
                        ext.FechaDesasignacion = null;
                    }
                    ext.FechaModificacion = now;
                    ext.UsuarioModificacion = usuario;
                }
                else
                {
                    _context.EmpresaGestores.Add(new EmpresaGestor
                    {
                        IdEmpresa = idEmpresa,
                        IdGestor = idG,
                        EsPrincipal = esPrincipal,
                        Activo = true,
                        FechaAsignacion = now,
                        FechaCreacion = now,
                        UsuarioCreacion = usuario
                    });
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
