// ConectaBiz.Infrastructure/DependencyInjection.cs
using System.Text;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Application.Services;
using ConectaBiz.Domain.Interfaces;
using ConectaBiz.Infrastructure.Authentication.Services;
using ConectaBiz.Infrastructure.Persistence.Contexts;
using ConectaBiz.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ConectaBiz.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuración de la base de datos
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // Repositorios
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IConsultorRepository, ConsultorRepository>();
            services.AddScoped<IPersonaRepository, PersonaRepository>();
            services.AddScoped<ITicketRepository, TicketRepository>();
            services.AddScoped<IFrenteRepository, FrenteRepository>();
            services.AddScoped<ISubFrenteRepository, SubFrenteRepository>();
            services.AddScoped<IConsultorFrenteSubFrenteRepository, ConsultorFrenteSubFrenteRepository>();
            services.AddScoped<IParametroRepository, ParametroRepository>();
            services.AddScoped<ITicketConsultorAsignacionRepository, TicketConsultorAsignacionRepository>();
            services.AddScoped<ITicketFrenteSubFrenteRepository, TicketFrenteSubFrenteRepository>();
            services.AddScoped<ITicketHistorialRepository, TicketHistorialRepository>();
            services.AddScoped<IEmpresaRepository, EmpresaRepository>();
            services.AddScoped<IPaisRepository, PaisRepository>();
            services.AddScoped<IGestorRepository, GestorRepository>();
            services.AddScoped<IGestorFrenteSubFrenteRepository, GestorFrenteSubFrenteRepository>();
            // Servicios
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IConsultorService, ConsultorService>();
            services.AddScoped<IPersonaService, PersonaService>();
            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<ISubFrenteService, SubFrenteService>();
            services.AddScoped<IFrenteService, FrenteService>();
            services.AddScoped<IParametroService, ParametroService>();
            services.AddScoped<IEmpresaService, EmpresaService>();
            services.AddScoped<IPaisService, PaisService>();
            services.AddScoped<IGestorService, GestorService>();

            // Configuración JWT
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                };
            });

            return services;
        }
    }
}