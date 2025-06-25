using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using ConectaBiz.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Infrastructure.Persistence.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly ApplicationDbContext _context;

        public TicketRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        //public async Task<IEnumerable<Ticket>> GetAllAsync()
        //{
        //    return await _context.Ticket
        //        .Include(t => t.ConsultorAsignaciones.Where(ca => ca.Activo))
        //        .Include(t => t.FrenteSubFrentes.Where(fsf => fsf.Activo))
        //        .ToListAsync();
        //}

        public async Task<IEnumerable<Ticket>> GetAllAsync()
        {
            try
            {
                var tickets = await _context.Ticket
                    .Include(t => t.ConsultorAsignaciones)
                    .Include(t => t.FrenteSubFrentes)
                    .ToListAsync();

                foreach (var ticket in tickets)
                {
                    ticket.ConsultorAsignaciones = ticket.ConsultorAsignaciones?
                        .Where(ca => ca.Activo).ToList();

                    ticket.FrenteSubFrentes = ticket.FrenteSubFrentes?
                        .Where(fsf => fsf.Activo).ToList();
                }

                return tickets;
            }
            catch (Exception ex)
            {

                throw;
            }
       
        }



        public async Task<Ticket?> GetByIdAsync(int id)
        {
            return await _context.Ticket.FindAsync(id);
        }

        public async Task<Ticket?> GetByIdWithRelationsAsync(int id)
        {
            return await _context.Ticket
                .Include(t => t.ConsultorAsignaciones.Where(fsf => fsf.Activo))
                .Include(t => t.FrenteSubFrentes.Where(fsf => fsf.Activo))
                .Include(t => t.TicketHistorialEstado)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Ticket?> GetByCodTicketAsync(string codTicket)
        {
            return await _context.Ticket
                .Include(t => t.ConsultorAsignaciones.Where(ca => ca.Activo))
                .Include(t => t.FrenteSubFrentes.Where(fsf => fsf.Activo))
                .FirstOrDefaultAsync(t => t.CodTicket == codTicket);
        }

        public async Task<IEnumerable<Ticket>> GetByEmpresaAsync(int idEmpresa)
        {
            return await _context.Ticket
                .Where(t => t.IdEmpresa == idEmpresa)
                .Include(t => t.ConsultorAsignaciones.Where(ca => ca.Activo))
                .Include(t => t.FrenteSubFrentes.Where(fsf => fsf.Activo))
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByEstadoAsync(int idEstado)
        {
            return await _context.Ticket
                .Where(t => t.IdEstadoTicket == idEstado)
                .Include(t => t.ConsultorAsignaciones.Where(ca => ca.Activo))
                .Include(t => t.FrenteSubFrentes.Where(fsf => fsf.Activo))
                .ToListAsync();
        }

        //public async Task<IEnumerable<Ticket>> GetByGestorAsync(int idGestor)
        //{
        //    return await _context.Ticket
        //        .Where(t => t.IdGestorAsignado == idGestor)
        //        .Include(t => t.ConsultorAsignaciones.Where(ca => ca.Activo))
        //        .Include(t => t.FrenteSubFrentes.Where(fsf => fsf.Activo))
        //        .ToListAsync();
        //}

        //public async Task<Ticket> CreateAsync(Ticket ticket)
        //{
        //    try
        //    {
        //        var a = ticket.Activo;
        //        _context.Ticket.Add(ticket);
        //        await _context.SaveChangesAsync();
        //        return ticket;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
   
        //}

        public async Task<Ticket> CreateAsync(Ticket ticket)
        {
            var logBuilder = new StringBuilder();

            // Clona las opciones del contexto actual, pero con logging en el StringBuilder
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql("Host=64.23.182.32;Port=5432;Database=conectabiz_db;Username=postgres;Password=KQW%9gVPK!+2kCh")
                .LogTo(s => logBuilder.AppendLine(s), LogLevel.Information)
                .EnableSensitiveDataLogging()
                .Options;

            // Crea una instancia temporal con logging activado
            using var context = new ApplicationDbContext(options);

            try
            {
                ticket.Activo = true;
                context.Ticket.Add(ticket);
                await context.SaveChangesAsync();

                // Aquí puedes obtener el SQL como string
                var sqlGenerado = logBuilder.ToString();
                Console.WriteLine("SQL generado:\n" + sqlGenerado);

                return ticket;
            }
            catch (Exception ex)
            {
                var sqlGenerado = logBuilder.ToString();
                Console.WriteLine("Error, SQL:\n" + sqlGenerado);
                throw;
            }
        }


        public async Task<Ticket> UpdateAsync(Ticket ticket)
        {
            _context.Ticket.Update(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket == null) return false;

            // Eliminación lógica
            ticket.Activo = false;
            ticket.FechaActualizacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> ExistsAsync(string codTicket, int? excludeId = null)
        {
            return await _context.Ticket
                .AnyAsync(t => t.CodTicket == codTicket && (excludeId == null || t.Id != excludeId));
        }

        public async Task<IEnumerable<Ticket>> GetTicketsWithFiltersAsync(int? idEmpresa = null, int? idEstado = null, bool? urgente = null)
        {
            var query = _context.Ticket.AsQueryable();

            if (idEmpresa.HasValue)
                query = query.Where(t => t.IdEmpresa == idEmpresa.Value);

            if (idEstado.HasValue)
                query = query.Where(t => t.IdEstadoTicket == idEstado.Value);

            return await query
                .Include(t => t.ConsultorAsignaciones.Where(ca => ca.Activo))
                .Include(t => t.FrenteSubFrentes.Where(fsf => fsf.Activo))
                .ToListAsync();
        }
        public async Task<IEnumerable<TicketConsultorAsignacion>> GetConsultorAsignacionesActivasByTicketIdAsync(int idTicket)
        {
            return await _context.Set<TicketConsultorAsignacion>()
                .Where(x => x.IdTicket == idTicket && x.Activo)
                .ToListAsync();
        }

        public async Task<IEnumerable<TicketFrenteSubFrente>> GetFrenteSubFrentesActivosByTicketIdAsync(int idTicket)
        {
            return await _context.Set<TicketFrenteSubFrente>()
                .Where(x => x.IdTicket == idTicket && x.Activo)
                .ToListAsync();
        }

        public async Task<Ticket?> GetByCodReqSgrCstiAsync(string codReqSgrCsti)
        {
            return await _context.Ticket
                .Include(t => t.ConsultorAsignaciones.Where(ca => ca.Activo))
                .Include(t => t.FrenteSubFrentes.Where(fsf => fsf.Activo))
                .FirstOrDefaultAsync(t => t.CodReqSgrCsti == codReqSgrCsti);
        }
    }
}
