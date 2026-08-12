using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Application.Services;

namespace ConectaBiz.API.Jobs
{
    public class RecurringJobWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public RecurringJobWorker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var sgrcstiService = scope.ServiceProvider.GetRequiredService<ISGRCSTIService>();
                        await sgrcstiService.MigracionRequerimientos();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RecurringJobWorker] Error ejecutando tarea programada: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);
            }
        }
    }
}