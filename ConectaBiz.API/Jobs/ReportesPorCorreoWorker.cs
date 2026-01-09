using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ConectaBiz.API.Jobs
{
    public class ReportesPorCorreoWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ReportesPorCorreoJobSettings _settings;

        public ReportesPorCorreoWorker(
            IServiceProvider serviceProvider,
            IOptions<ReportesPorCorreoJobSettings> options)
        {
            _serviceProvider = serviceProvider;
            _settings = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Espera hasta las 08:00 (o la hora configurada)
                    var nextRun = GetNextRun(DateTime.Now, _settings.HoraEjecucion);
                    var delay = nextRun - DateTime.Now;

                    if (delay > TimeSpan.Zero)
                        await Task.Delay(delay, stoppingToken);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var reportesService = scope.ServiceProvider.GetRequiredService<IReportesService>();

                        await reportesService.Enviar3ExcelsAsync(
                            _settings.FechaDesdeAutorizados,
                            _settings.FechaDesdeNoCerrados,
                            _settings.Emails
                        );
                    }
                }
                catch (Exception ex)
                {
                    // Aquí puedes loguear el error si tienes un logger
                    // Ej: _logger.LogError(ex, "Error en ReportesPorCorreoWorker");
                }

                // Evita doble ejecución el mismo día: duerme 24h
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private static DateTime GetNextRun(DateTime now, string horaEjecucion)
        {
            var parts = horaEjecucion.Split(':');
            var hour = int.Parse(parts[0]);
            var minute = int.Parse(parts[1]);

            var scheduled = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);

            return scheduled <= now ? scheduled.AddDays(1) : scheduled;
        }
    }
}
