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
    public class TicketFrenteSubFrenteRepository : ITicketFrenteSubFrenteRepository
    {
        private readonly ApplicationDbContext _context;

        public TicketFrenteSubFrenteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TicketFrenteSubFrente>> GetByTicketIdAsync(int idTicket)
        {
            return await _context.TicketFrenteSubFrente
                .Where(tfsf => tfsf.IdTicket == idTicket)
                .ToListAsync();
        }

        public async Task<IEnumerable<TicketFrenteSubFrente>> GetActivosByTicketIdAsync(int idTicket)
        {
            return await _context.TicketFrenteSubFrente
                .Where(tfsf => tfsf.IdTicket == idTicket && tfsf.Activo)
                .ToListAsync();
        }

        public async Task<TicketFrenteSubFrente> CreateAsync(TicketFrenteSubFrente frenteSubFrente)
        {
            _context.TicketFrenteSubFrente.Add(frenteSubFrente);
            await _context.SaveChangesAsync();
            return frenteSubFrente;
        }
        public async Task<TicketFrenteSubFrente> UpdateAsync(TicketFrenteSubFrente frenteSubFrente)
        {
            _context.TicketFrenteSubFrente.Update(frenteSubFrente);
            await _context.SaveChangesAsync();
            return frenteSubFrente;
        }

        public async Task<IEnumerable<TicketFrenteSubFrente>> CreateRangeAsync(IEnumerable<TicketFrenteSubFrente> frentesSubFrentes)
        {
            if (frentesSubFrentes == null || !frentesSubFrentes.Any())
                throw new ArgumentException("La lista no puede estar vacía.");

            await _context.TicketFrenteSubFrente.AddRangeAsync(frentesSubFrentes);
            await _context.SaveChangesAsync();

            return frentesSubFrentes;
        }

        public async Task<IEnumerable<TicketFrenteSubFrente>> UpdateRangeAsync(IEnumerable<TicketFrenteSubFrente> frentesSubFrentes)
        {
            try
            {
                if (frentesSubFrentes == null || !frentesSubFrentes.Any())
                    throw new ArgumentException("La lista no puede estar vacía.");

                // Iteramos sobre las entidades mapeadas
                foreach (var entidad in frentesSubFrentes)
                {
                    // Verificamos si ya hay una entidad con el mismo Id trackeada
                    var tracked = _context.ChangeTracker.Entries<TicketFrenteSubFrente>()
                        .FirstOrDefault(e => e.Entity.Id == entidad.Id);

                    if (tracked != null)
                    {
                        // Sobrescribimos los valores de la entidad trackeada con los del mapper
                        tracked.CurrentValues.SetValues(entidad);

                        // Indicamos que los campos de creación no se deben modificar
                        tracked.Property(x => x.UsuarioCreacion).IsModified = false;
                        tracked.Property(x => x.FechaCreacion).IsModified = false;
                    }
                    else
                    {
                        // Si no está trackeada, la adjuntamos y marcamos como Modified
                        _context.TicketFrenteSubFrente.Attach(entidad);
                        _context.Entry(entidad).State = EntityState.Modified;

                        // Evitamos modificar los campos de creación
                        _context.Entry(entidad).Property(x => x.UsuarioCreacion).IsModified = false;
                        _context.Entry(entidad).Property(x => x.FechaCreacion).IsModified = false;
                    }
                }

                await _context.SaveChangesAsync();

                return frentesSubFrentes;
            }
            catch (Exception ex)
            {
                throw;
            }
        }





        public async Task<bool> DeactivateAllByTicketIdAsync(int idTicket, string usuarioModificacion)
        {
            var frenteSubFrentes = await _context.TicketFrenteSubFrente
                .Where(tfsf => tfsf.IdTicket == idTicket && tfsf.Activo)
                .ToListAsync();

            foreach (var item in frenteSubFrentes)
            {
                item.Activo = false;
                item.FechaModificacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                item.FechaCreacion = DateTime.SpecifyKind(item.FechaCreacion, DateTimeKind.Local);
                item.UsuarioModificacion = usuarioModificacion;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var frenteSubFrente = await _context.TicketFrenteSubFrente.FindAsync(id);
            if (frenteSubFrente == null) return false;

            // Eliminación lógica
            frenteSubFrente.Activo = false;
            frenteSubFrente.FechaModificacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

            await _context.SaveChangesAsync();
            return true;
        }
    }

}
