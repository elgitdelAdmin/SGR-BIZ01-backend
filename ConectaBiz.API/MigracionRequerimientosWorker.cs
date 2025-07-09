using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using ConectaBiz.Application.Services;

public class MigracionRequerimientosWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public MigracionRequerimientosWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //while (!stoppingToken.IsCancellationRequested)
        //{
        //    try
        //    {
        //        using (var scope = _serviceProvider.CreateScope())
        //        {
        //            var sgrcstiService = scope.ServiceProvider.GetRequiredService<SGRCSTIService>();
        //            await sgrcstiService.MigracionRequerimientos();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Aquí puedes loguear el error si tienes un logger
        //    }

        //    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        //}
    }
} 