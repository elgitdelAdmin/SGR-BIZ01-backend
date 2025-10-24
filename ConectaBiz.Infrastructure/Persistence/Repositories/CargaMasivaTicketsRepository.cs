using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using ConectaBiz.Infrastructure.Persistence.Contexts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Infrastructure.Persistence.Repositories
{
    public class CargaMasivaTicketsRepository : ICargaMasivaTicketsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly string _rutaLog;
        public CargaMasivaTicketsRepository(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _rutaLog = configuration["Logging:LogFilePath"];
        }

        public Task<List<Ticket>> InsertarIncidentesAlicorpAsync(List<Ticket> tickets)
        {
            throw new NotImplementedException();
        }
    }
}
