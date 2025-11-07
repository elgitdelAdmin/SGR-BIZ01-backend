using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
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
                        var sgrcstiService = scope.ServiceProvider.GetRequiredService<SGRCSTIService>();
                        await sgrcstiService.MigracionRequerimientos();
                    }
                }
                catch (Exception ex)
                {
                    // Aquí puedes loguear el error si tienes un logger
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
} 