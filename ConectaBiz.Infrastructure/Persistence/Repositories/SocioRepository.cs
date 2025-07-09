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
    }

}
