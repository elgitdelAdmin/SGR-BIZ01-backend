using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Constants;
using ConectaBiz.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Infrastructure.Caching
{
    public sealed class ParametrosCatalogo : IParametrosCatalogo
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ParametrosCatalogo> _logger;

        private readonly SemaphoreSlim _lock = new(1, 1);

        private ParametrosSnapshot _current = new();
        private DateTime _loadedAtUtc = DateTime.MinValue;

        // Ajusta el TTL según tu realidad
        private readonly TimeSpan _ttl = TimeSpan.FromHours(12);

        public ParametrosCatalogo(IServiceScopeFactory scopeFactory, ILogger<ParametrosCatalogo> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public ParametrosSnapshot Current => _current;

        public async Task EnsureLoadedAsync(CancellationToken ct = default)
        {
            if (_loadedAtUtc != DateTime.MinValue && (DateTime.UtcNow - _loadedAtUtc) < _ttl)
                return;

            await LoadInternalAsync(force: false, ct);
        }

        public Task RefreshAsync(CancellationToken ct = default)
            => LoadInternalAsync(force: true, ct);

        private async Task LoadInternalAsync(bool force, CancellationToken ct)
        {
            if (!force && _loadedAtUtc != DateTime.MinValue && (DateTime.UtcNow - _loadedAtUtc) < _ttl)
                return;

            await _lock.WaitAsync(ct);
            try
            {
                if (!force && _loadedAtUtc != DateTime.MinValue && (DateTime.UtcNow - _loadedAtUtc) < _ttl)
                    return;

                using var scope = _scopeFactory.CreateScope();

                var parametroRepository = scope.ServiceProvider.GetRequiredService<IParametroRepository>();
                var listaParametros = (await parametroRepository.GetAllAsync()).ToList();

                var snapshot = new ParametrosSnapshot
                {
                    ListaParametros = listaParametros,
                    ListaTipoTicket = listaParametros
                        .Where(p => p.TipoParametro == AppConstants.TiposParametros.TipoTicket)
                        .ToList(),
                    ListaSubTipoTicket = listaParametros
                        .Where(p => p.TipoParametro == AppConstants.TiposParametros.SubTipoTicket)
                        .ToList(),
                    ListaEstados = listaParametros
                        .Where(p => p.TipoParametro == AppConstants.TiposParametros.EstadoTicket)
                        .ToList(),
                    ListaPrioridades = listaParametros
                        .Where(p => p.TipoParametro == AppConstants.TiposParametros.Prioridad)
                        .ToList(),
                    ListaTipoActividad = listaParametros
                        .Where(p => p.TipoParametro == AppConstants.TiposParametros.TipoActividad)
                        .ToList(),
                    ListaReportes = listaParametros
                        .Where(p => p.TipoParametro == AppConstants.TiposParametros.TipoReporte)
                        .ToList(),
                };

                _current = snapshot;
                _loadedAtUtc = DateTime.UtcNow;

                _logger.LogInformation("ParametrosCatalogo cargado. Total: {Total} - {FechaUtc}",
                    snapshot.ListaParametros.Count, _loadedAtUtc);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando ParametrosCatalogo");
                throw;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
