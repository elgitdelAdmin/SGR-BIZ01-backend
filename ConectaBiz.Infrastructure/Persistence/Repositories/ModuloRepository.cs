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
    public class ModuloRepository : IModuloRepository
    {
        private readonly ApplicationDbContext _context;

        public ModuloRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Modulo>> GetAllAsync()
        {
            return await _context.Modulos.ToListAsync();
        }

        public async Task<List<RolPermisoModulo>> GetByRolAsync(int idRol)
        {
            return await _context.RolPermisoModulos
                .Include(rp => rp.Modulo)
                .Where(rp => rp.IdRol == idRol)
                .ToListAsync();
        }
    }

}
