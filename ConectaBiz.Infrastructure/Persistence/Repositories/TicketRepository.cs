using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using ConectaBiz.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace ConectaBiz.Infrastructure.Persistence.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly string _rutaLog;

        public TicketRepository(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _rutaLog = configuration["Logging:LogFilePath"];
        }

        public async Task<IEnumerable<Ticket>> GetAllAsync()
        {
            try
            {
                var tickets = await _context.Ticket
                    .Include(t => t.Empresa)
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
                .Include(t => t.ConsultorAsignaciones.Where(fsf => fsf.Activo)).ThenInclude(ca => ca.DetalleTareasConsultor.Where(dt => dt.Activo))
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
                .Include(t => t.Empresa)
                .Include(t => t.ConsultorAsignaciones.Where(ca => ca.Activo))
                .Include(t => t.FrenteSubFrentes.Where(fsf => fsf.Activo))
                .ToListAsync();
        }
        public async Task<IEnumerable<Ticket>> GetByIdSocioNumContribuyenteEmpAsync(int idSocio, string numContribuyenteEmp)
        {
            return await _context.Ticket
                .Include(t => t.Empresa)
                .Include(t => t.ConsultorAsignaciones.Where(ca => ca.Activo))
                .Include(t => t.FrenteSubFrentes.Where(fsf => fsf.Activo))
                .Where(t =>
                    t.Empresa != null &&
                    t.Empresa.IdSocio == idSocio &&
                    t.Empresa.NumDocContribuyente == numContribuyenteEmp)
                .ToListAsync();
        }
        public async Task<IEnumerable<Ticket>> GetByNumContribuyenteSocioEmpAsync(string numContribuyenteSocio, string numContribuyenteEmp)
        {
            return await _context.Ticket
                .Where(t =>
                    t.Empresa != null &&
                    t.Empresa.Socio != null &&
                    t.Empresa.Socio.NumDocContribuyente == numContribuyenteSocio &&
                    t.Empresa.NumDocContribuyente == numContribuyenteEmp &&
                    t.Activo)
                .Select(t => new Ticket
                {
                    CodTicketInterno = t.CodTicketInterno
                })
                .AsNoTracking()
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
        public async Task<IEnumerable<Ticket>> GetByGestorAsync(int idGestor)
        {
            // Lista de gestores válidos: el recibido por parámetro (puedes agregar otros si deseas)
            var idsGestores = new List<int> { idGestor };

            return await _context.Ticket
                .Where(t => t.IdGestor.HasValue && idsGestores.Contains(t.IdGestor.Value))
                .Include(t => t.Empresa)
                .Include(t => t.ConsultorAsignaciones.Where(ca => ca.Activo))
                    .ThenInclude(ca => ca.DetalleTareasConsultor.Where(dt => dt.Activo)) // 👈 Incluye DetalleTareasConsultor
                .Include(t => t.FrenteSubFrentes.Where(fsf => fsf.Activo))
                .ToListAsync();
        }


        public async Task<IEnumerable<Ticket>> GetByGestorConsultoriaAsync(int idGestor)
        {
            return await _context.Ticket
                .Where(t => t.IdGestorConsultoria == idGestor)
                .Include(t => t.Empresa)
                .Include(t => t.ConsultorAsignaciones.Where(ca => ca.Activo))
                .Include(t => t.FrenteSubFrentes.Where(fsf => fsf.Activo))
                .ToListAsync();
        }
        public async Task<IEnumerable<Ticket>> GetByConsultorAsync(int idConsultor)
        {
            return await _context.Ticket
                .Where(t => t.Activo &&
                            t.ConsultorAsignaciones.Any(ca => ca.IdConsultor == idConsultor && ca.Activo))
                .Include(t => t.Empresa)
                .Include(t => t.ConsultorAsignaciones
                    .Where(ca => ca.Activo && ca.IdConsultor == idConsultor))
                    .ThenInclude(ca => ca.DetalleTareasConsultor
                        .Where(dt => dt.Activo))
                .Include(t => t.FrenteSubFrentes.Where(fsf => fsf.Activo))
                .ToListAsync();
        }


        public async Task<Ticket> CreateAsync(Ticket ticket)
        {
            try
            {
                ticket.Activo = true;
                _context.Ticket.Add(ticket);
                await _context.SaveChangesAsync();
                return ticket;
            }
            catch (DbUpdateException ex)
            {
                await File.AppendAllTextAsync(_rutaLog, ex.InnerException?.Message);
                throw;
            }
        }
        public async Task<List<Ticket>> CreateRangeAsync(List<Ticket> tickets)
        {
            if (tickets == null || !tickets.Any())
                return new List<Ticket>();

            try
            {
                tickets.ForEach(t => t.Activo = true);
                _context.Ticket.AddRange(tickets);
                await _context.SaveChangesAsync();
                return tickets;
            }
            catch (DbUpdateException ex)
            {
                // Ruta del archivo de log
                var rutaLog = Path.Combine(AppContext.BaseDirectory, "tickets_error_log.txt");

                // Acumulamos todo el texto en un StringBuilder
                var sb = new System.Text.StringBuilder();

                foreach (var t in tickets)
                {
                    foreach (var prop in t.GetType().GetProperties())
                    {
                        // Solo propiedades string y que NO sean Descripcion
                        if (prop.PropertyType == typeof(string) && prop.Name != "Descripcion")
                        {
                            var value = prop.GetValue(t)?.ToString() ?? "";
                            if (value.Length > 50) // Solo los que pueden exceder varchar(50)
                            {
                                sb.AppendLine($"Posible problema: {prop.Name} = '{value}' ({value.Length} chars)");
                            }
                        }
                    }
                }
                var ff = sb.ToString();
                // Escribir todo el log **una sola vez**
                await File.AppendAllTextAsync(rutaLog, sb.ToString() + Environment.NewLine);

                throw; // Re-lanzamos la excepción para que la Web API la maneje
            }
        }





        public async Task<Ticket> UpdateAsync(Ticket ticket)
        {
            try
            {
                _context.Ticket.Update(ticket);
                await _context.SaveChangesAsync();
                return ticket;
            }
            catch (DbUpdateException ex)
            {
                await File.AppendAllTextAsync(_rutaLog, ex.InnerException?.Message);
                throw;
            }
        }

        public async Task<IEnumerable<Ticket>> UpdateRangeAsync(IEnumerable<Ticket> tickets)
        {
            _context.Ticket.UpdateRange(tickets);
            await _context.SaveChangesAsync();
            return tickets;
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
            return await _context.TicketConsultorAsignacion
                .AsNoTracking() // 👈 Esto evita el rastreo
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
