using System;
using System.Threading;
using System.Threading.Tasks;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConectaBiz.API.Jobs
{
    public class WhatsAppNotificationWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WhatsAppNotificationWorker> _logger;
        private readonly WhatsAppJobSettings _settings;

        public WhatsAppNotificationWorker(
            IServiceProvider serviceProvider,
            ILogger<WhatsAppNotificationWorker> logger,
            IOptions<WhatsAppJobSettings> options)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _settings = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WhatsAppNotificationWorker iniciado. Intervalo de ejecución: {Intervalo} minutos.", _settings.IntervaloMinutos);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificacionWhatsAppService>();
                        _logger.LogInformation("Iniciando envío periódico de WhatsApp...");
                        await notificationService.EnviarNotificacionesWhatsAppAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en la ejecución del Job periódico de WhatsApp.");
                }

                var delayMinutes = _settings.IntervaloMinutos > 0 ? _settings.IntervaloMinutos : 60;
                await Task.Delay(TimeSpan.FromMinutes(delayMinutes), stoppingToken);
            }
        }
    }
}
