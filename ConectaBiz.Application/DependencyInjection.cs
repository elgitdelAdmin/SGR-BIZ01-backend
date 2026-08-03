using System.Reflection;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ConectaBiz.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
        {
            // Registrar AutoMapper escaneando el ensamblado actual
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Registrar los servicios de aplicación
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IConsultorService, ConsultorService>();
            services.AddScoped<IPersonaService, PersonaService>();
            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<ISubFrenteService, SubFrenteService>();
            services.AddScoped<IFrenteService, FrenteService>();
            services.AddScoped<IParametroService, ParametroService>();
            services.AddScoped<IEmpresaService, EmpresaService>();
            services.AddScoped<IPaisService, PaisService>();
            services.AddScoped<IGestorService, GestorService>();
            services.AddScoped<IModuloService, ModuloService>();
            services.AddScoped<ISocioService, SocioService>();
            services.AddScoped<INotificacionTicketService, NotificacionTicketService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<INotificacionWhatsAppService, NotificacionWhatsAppService>();
            services.AddScoped<ISGRCSTIService, SGRCSTIService>();

            // Registrar Lazy para dependencias circulares (si aplican)
            services.AddScoped(provider =>
                new Lazy<INotificacionTicketService>(
                    () => provider.GetRequiredService<INotificacionTicketService>()
                )
            );
            services.AddScoped(provider =>
                new Lazy<ITicketService>(() =>
                    provider.GetRequiredService<ITicketService>()
                )
            );

            return services;
        }
    }
}
