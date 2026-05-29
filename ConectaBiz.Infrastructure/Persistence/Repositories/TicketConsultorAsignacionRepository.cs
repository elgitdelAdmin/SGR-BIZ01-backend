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

        /// <summary>
        /// Retorna las asignaciones activas del ticket incluyendo sus detalles de planificación.
        /// Usado para auto-vincular IdDetallePlanificacionConsultor en TicketFrenteSubFrente.
        /// </summary>
        public async Task<IEnumerable<TicketConsultorAsignacion>> GetActivosConPlanificacionByTicketIdAsync(int idTicket)
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
        public async Task<IEnumerable<DetallePlanificacionConsultor>> CreatePlanificacionRangeAsync(List<DetallePlanificacionConsultor> detallesPlanificacion)
        {
            try
            {
                if (detallesPlanificacion == null || detallesPlanificacion.Count == 0)
                    return new List<DetallePlanificacionConsultor>();

                // Asegúrate que todos los Id sean 0 para inserción
                foreach (var planificacion in detallesPlanificacion)
                {
                    planificacion.Id = 0; // Forzar inserción
                }
                await _context.DetallePlanificacionConsultor.AddRangeAsync(detallesPlanificacion);
                await _context.SaveChangesAsync();

                return detallesPlanificacion;
            }
            catch (Exception ex)
            {
                // Log del error con más detalle
                Console.WriteLine($"Error al guardar planificacion: {ex.Message}");
                Console.WriteLine($"Cantidad de planificacion: {detallesPlanificacion?.Count ?? 0}");
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
                var existentesIds = await _context.TicketConsultorAsignacion
                    .Where(x => ids.Contains(x.Id))
                    .Select(x => x.Id)
                    .ToListAsync();

                var filtradas = asignaciones.Where(x => existentesIds.Contains(x.Id)).ToList();
                if (filtradas.Count == 0)
                    return asignaciones;

                // Detach las entidades que podrían estar siendo rastreadas
                var trackedEntities = _context.ChangeTracker.Entries<TicketConsultorAsignacion>()
                    .Where(e => existentesIds.Contains(e.Entity.Id))
                    .ToList();

                foreach (var entity in trackedEntities)
                {
                    entity.State = EntityState.Detached;
                }

                _context.TicketConsultorAsignacion.UpdateRange(filtradas);
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
                var existentesIds = await _context.DetalleTareasConsultor
                    .Where(x => ids.Contains(x.Id))
                    .Select(x => x.Id)
                    .ToListAsync();

                var filtradas = detallesTareas.Where(x => existentesIds.Contains(x.Id)).ToList();
                if (filtradas.Count == 0)
                    return detallesTareas;

                // Detach las entidades que podrían estar siendo rastreadas
                var trackedEntities = _context.ChangeTracker.Entries<DetalleTareasConsultor>()
                    .Where(e => existentesIds.Contains(e.Entity.Id))
                    .ToList();

                foreach (var entity in trackedEntities)
                {
                    entity.State = EntityState.Detached;
                }

                _context.DetalleTareasConsultor.UpdateRange(filtradas);
                await _context.SaveChangesAsync();

                return detallesTareas;
            }
            catch (Exception ex)
            {
                // Aquí puedes loggear ex si lo deseas
                throw;
            }
        }
        public async Task<IEnumerable<DetallePlanificacionConsultor>> UpdatePlanificacionRangeAsync(List<DetallePlanificacionConsultor> detallesPlanificacion)
        {
            try
            {
                if (detallesPlanificacion == null || detallesPlanificacion.Count == 0)
                    return new List<DetallePlanificacionConsultor>();

                // Obtener los IDs que vamos a actualizar
                var ids = detallesPlanificacion.Select(x => x.Id).ToList();
                var existentesIds = await _context.DetallePlanificacionConsultor
                    .Where(x => ids.Contains(x.Id))
                    .Select(x => x.Id)
                    .ToListAsync();

                var filtradas = detallesPlanificacion.Where(x => existentesIds.Contains(x.Id)).ToList();
                if (filtradas.Count == 0)
                    return detallesPlanificacion;

                // Detach las entidades que podrían estar siendo rastreadas
                var trackedEntities = _context.ChangeTracker.Entries<DetallePlanificacionConsultor>()
                    .Where(e => existentesIds.Contains(e.Entity.Id))
                    .ToList();

                foreach (var entity in trackedEntities)
                {
                    entity.State = EntityState.Detached;
                }

                _context.DetallePlanificacionConsultor.UpdateRange(filtradas);
                await _context.SaveChangesAsync();

                return detallesPlanificacion;
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

        public async Task<bool> AnyPlanificacionActivaAsync(int idPlanificacion)
        {
            return await _context.DetallePlanificacionConsultor
                .AnyAsync(dp => dp.Id == idPlanificacion && dp.Activo);
        }

        public async Task<IEnumerable<DetallePlanificacionConsultor>> GetPlanificacionesByIdsAsync(IEnumerable<int> ids)
        {
            if (ids == null || !ids.Any()) return new List<DetallePlanificacionConsultor>();
            return await _context.DetallePlanificacionConsultor
                .Where(dp => ids.Contains(dp.Id) && dp.Activo)
                .ToListAsync();
        }

        public async Task<IEnumerable<DetallePlanificacionConsultor>> GetPlanificacionesByFrenteIdsAsync(IEnumerable<int> frenteIds)
        {
            if (frenteIds == null || !frenteIds.Any()) return new List<DetallePlanificacionConsultor>();
            return await _context.DetallePlanificacionConsultor
                .Where(dp => dp.Activo && frenteIds.Contains(dp.IdTicketFrenteSubFrente))
                .ToListAsync();
        }
    }
}
