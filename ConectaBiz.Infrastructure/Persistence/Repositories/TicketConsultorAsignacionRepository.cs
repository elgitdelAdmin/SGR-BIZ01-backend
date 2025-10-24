using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using ConectaBiz.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Infrastructure.Persistence.Repositories
{
    public class TicketConsultorAsignacionRepository : ITicketConsultorAsignacionRepository
    {
        private readonly ApplicationDbContext _context;

        public TicketConsultorAsignacionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TicketConsultorAsignacion>> GetByTicketIdAsync(int idTicket)
        {
            return await _context.TicketConsultorAsignacion
                .Where(tca => tca.IdTicket == idTicket)
                .ToListAsync();
        }

        public async Task<IEnumerable<TicketConsultorAsignacion>> GetActivosByTicketIdAsync(int idTicket)
        {
            return await _context.TicketConsultorAsignacion
                .Where(tca => tca.IdTicket == idTicket && tca.Activo)
                .ToListAsync();
        }

        public async Task<TicketConsultorAsignacion> CreateAsync(TicketConsultorAsignacion asignacion)
        {
            try
            {
                _context.TicketConsultorAsignacion.Add(asignacion);
                await _context.SaveChangesAsync();
                return asignacion;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public async Task<IEnumerable<TicketConsultorAsignacion>> CreateRangeAsync(List<TicketConsultorAsignacion> asignaciones)
        {
            try
            {
                if (asignaciones == null || asignaciones.Count == 0)
                    return new List<TicketConsultorAsignacion>();

                // Si quieres, puedes asegurar que ninguna entidad con el mismo Id ya esté siendo rastreada
                var ids = asignaciones.Where(a => a.Id != 0).Select(x => x.Id).ToList();
                var trackedEntities = _context.ChangeTracker.Entries<TicketConsultorAsignacion>()
                    .Where(e => ids.Contains(e.Entity.Id))
                    .ToList();

                foreach (var entity in trackedEntities)
                {
                    entity.State = EntityState.Detached;
                }

                await _context.TicketConsultorAsignacion.AddRangeAsync(asignaciones);
                await _context.SaveChangesAsync();

                return asignaciones;
            }
            catch (Exception ex)
            {
                // Aquí podrías loggear ex antes de relanzar
                throw;
            }
        }


        public async Task<IEnumerable<DetalleTareasConsultor>> CreateTareasRangeAsync(List<DetalleTareasConsultor> detallesTareas)
        {
            try
            {
                if (detallesTareas == null || detallesTareas.Count == 0)
                    return new List<DetalleTareasConsultor>();

                // Asegúrate que todos los Id sean 0 para inserción
                foreach (var tarea in detallesTareas)
                {
                    tarea.Id = 0; // Forzar inserción
                }
                await _context.DetalleTareasConsultor.AddRangeAsync(detallesTareas);
                await _context.SaveChangesAsync();

                return detallesTareas;
            }
            catch (Exception ex)
            {
                // Log del error con más detalle
                Console.WriteLine($"Error al guardar tareas: {ex.Message}");
                Console.WriteLine($"Cantidad de tareas: {detallesTareas?.Count ?? 0}");
                throw;
            }
        }


        public async Task<TicketConsultorAsignacion> UpdateAsync(TicketConsultorAsignacion asignacion)
        {
            _context.TicketConsultorAsignacion.Update(asignacion);
            await _context.SaveChangesAsync();
            return asignacion;
        }
        public async Task<IEnumerable<TicketConsultorAsignacion>> UpdateRangeAsync(List<TicketConsultorAsignacion> asignaciones)
        {
            try
            {
                if (asignaciones == null || asignaciones.Count == 0)
                    return new List<TicketConsultorAsignacion>();

                // Obtener los IDs que vamos a actualizar
                var ids = asignaciones.Select(x => x.Id).ToList();

                // Detach las entidades que podrían estar siendo rastreadas
                var trackedEntities = _context.ChangeTracker.Entries<TicketConsultorAsignacion>()
                    .Where(e => ids.Contains(e.Entity.Id))
                    .ToList();

                foreach (var entity in trackedEntities)
                {
                    entity.State = EntityState.Detached;
                }

                _context.TicketConsultorAsignacion.UpdateRange(asignaciones);
                await _context.SaveChangesAsync();
                return asignaciones;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<IEnumerable<DetalleTareasConsultor>> UpdateTareasRangeAsync(List<DetalleTareasConsultor> detallesTareas)
        {
            try
            {
                if (detallesTareas == null || detallesTareas.Count == 0)
                    return new List<DetalleTareasConsultor>();

                // Obtener los IDs que vamos a actualizar
                var ids = detallesTareas.Select(x => x.Id).ToList();

                // Detach las entidades que podrían estar siendo rastreadas
                var trackedEntities = _context.ChangeTracker.Entries<DetalleTareasConsultor>()
                    .Where(e => ids.Contains(e.Entity.Id))
                    .ToList();

                foreach (var entity in trackedEntities)
                {
                    entity.State = EntityState.Detached;
                }

                _context.DetalleTareasConsultor.UpdateRange(detallesTareas);
                await _context.SaveChangesAsync();

                return detallesTareas;
            }
            catch (Exception ex)
            {
                // Aquí puedes loggear ex si lo deseas
                throw;
            }
        }

        public async Task<bool> DeactivateAllByTicketIdAsync(int idTicket, string usuarioDesasignacion)
        {
            var asignaciones = await _context.TicketConsultorAsignacion
                .Where(tca => tca.IdTicket == idTicket && tca.Activo)
                .ToListAsync();

            foreach (var asignacion in asignaciones)
            {
                asignacion.Activo = false;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var asignacion = await _context.TicketConsultorAsignacion.FindAsync(id);
            if (asignacion == null) return false;

            // Eliminación lógica
            asignacion.Activo = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
