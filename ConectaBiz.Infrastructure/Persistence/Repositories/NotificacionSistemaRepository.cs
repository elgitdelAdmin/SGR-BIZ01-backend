using System.Threading.Tasks;
using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using ConectaBiz.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ConectaBiz.Infrastructure.Persistence.Repositories
{
    public class NotificacionSistemaRepository : INotificacionSistemaRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public NotificacionSistemaRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<NotificacionSistema> AddAsync(NotificacionSistema notificacion)
        {
            await _dbContext.NotificacionesSistema.AddAsync(notificacion);
            await _dbContext.SaveChangesAsync();
            return notificacion;
        }

        public async Task AddRangeAsync(System.Collections.Generic.IEnumerable<NotificacionSistema> notificaciones)
        {
            try
            {
                await _dbContext.NotificacionesSistema.AddRangeAsync(notificaciones);
                await _dbContext.SaveChangesAsync();
            }
            catch (System.Exception ex)
            {
                throw;
            }
        }

        public async Task<System.Collections.Generic.IEnumerable<NotificacionSistema>> GetByReferenciaAndUsersAsync(int idReferencia, int[] idUsers)
        {
            return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
                System.Linq.Queryable.Where(_dbContext.NotificacionesSistema, n => n.IdReferencia == idReferencia && n.IdUser.HasValue && idUsers.Contains(n.IdUser.Value) && n.Activo)
            );
        }

        public async Task<System.Collections.Generic.IEnumerable<NotificacionSistema>> GetNotificacionesByUserIdAsync(int idUser)
        {
            return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
                System.Linq.Queryable.OrderByDescending(
                    System.Linq.Queryable.Where(_dbContext.NotificacionesSistema, n => n.IdUser == idUser && n.Activo),
                    n => n.FechaCreacion)
            );
        }

        public async Task<System.Collections.Generic.IEnumerable<NotificacionSistema>> GetNotificacionesNoLeidasByUserIdAsync(int idUser)
        {
            return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
                System.Linq.Queryable.OrderByDescending(
                    System.Linq.Queryable.Where(_dbContext.NotificacionesSistema, n => n.IdUser == idUser && !n.Leido && n.Activo),
                    n => n.FechaCreacion)
            );
        }

        public async Task<NotificacionSistema> GetByIdAsync(int id)
        {
            return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                _dbContext.NotificacionesSistema, n => n.Id == id && n.Activo);
        }

        public async Task UpdateAsync(NotificacionSistema notificacion)
        {
            _dbContext.NotificacionesSistema.Update(notificacion);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(System.Collections.Generic.IEnumerable<NotificacionSistema> notificaciones)
        {
            _dbContext.NotificacionesSistema.UpdateRange(notificaciones);
            await _dbContext.SaveChangesAsync();
        }

        public async Task MarcarComoLeidaAsync(int idUser, System.Collections.Generic.List<int> idsNotificaciones)
        {
            try
            {
                var notificaciones = await _dbContext.NotificacionesSistema
                .Where(n => idsNotificaciones.Contains(n.Id) && n.IdUser == idUser && n.Activo)
                .ToListAsync();

                foreach (var notificacion in notificaciones)
                {
                    notificacion.Leido = true;
                    notificacion.FechaLectura = System.DateTime.UtcNow;
                }
                
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task EliminarLogicaAsync(int id)
        {
            var notificacion = await GetByIdAsync(id);
            if (notificacion != null)
            {
                notificacion.Activo = false;
                _dbContext.NotificacionesSistema.Update(notificacion);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
