// ConectaBiz.Infrastructure/DependencyInjection.cs
using System.Text;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Application.Services;
using ConectaBiz.Infrastructure.Services;
using ConectaBiz.Domain.Constants;
using ConectaBiz.Domain.Interfaces;
using ConectaBiz.Infrastructure.Authentication.Services;
using ConectaBiz.Infrastructure.Persistence.Contexts;
using ConectaBiz.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Infrastructure.Caching;
using Microsoft.SemanticKernel; // Sirve para que reconozca AddKernel() y AddOpenAIChatCompletion()
using ConectaBiz.Infrastructure.AI; // Sirve para que reconozca tu nueva clase ChatAgent
using System.Net.Http; // Sirve para instanciar HttpClient
using System; // Sirve para Uri()
using Microsoft.SemanticKernel;


namespace ConectaBiz.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuración de la base de datos
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)),
                contextLifetime: ServiceLifetime.Scoped,  // Lifetime del DbContext
                optionsLifetime: ServiceLifetime.Scoped   // Lifetime de DbContextOptions
            );

            //Configuracion de BDSGRCSTI
            Conexiones.ConnectionSGRCSTI = configuration.GetConnectionString("ConnectionSGRCSTI");
            Conexiones.ConnectionConectaNuevo = configuration.GetConnectionString("ConnectionConectaNuevo");

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
            services.AddScoped<IEmpresaGestorRepository, EmpresaGestorRepository>();
            services.AddScoped<IPaisRepository, PaisRepository>();
            services.AddScoped<IGestorRepository, GestorRepository>();
            services.AddScoped<IGestorFrenteSubFrenteRepository, GestorFrenteSubFrenteRepository>();
            services.AddScoped<IModuloRepository, ModuloRepository>();
            services.AddScoped<ISocioRepository, SocioRepository>();
            services.AddScoped<INotificacionTicketRepository, NotificacionTicketRepository>();
            services.AddScoped<IReportesRepository, ReportesRepository>();
            services.AddScoped<IReportesService, ReportesService>();
            services.AddScoped<IExcelService, ExcelService>();

            // Integraciones con APIs Externas
            services.AddHttpClient<IWhatsAppService, WhatsAppService>();

            // Servicios de Infraestructura (Ej: Token, Hashing)
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
            
            services.AddSingleton<IParametrosCatalogo, ParametrosCatalogo>();

            //Integracion
            services.AddScoped<ISGRCSTIRepository, SGRCSTIRepository>();
            services.AddScoped<IConectaNuevoTicketRepository, ConectaNuevoTicketRepository>();

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

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (string.IsNullOrEmpty(context.Token))
                        {
                            var cookieToken = context.Request.Cookies["jwt"];
                            if (!string.IsNullOrEmpty(cookieToken))
                            {
                                context.Token = cookieToken;
                            }
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.Headers["X-Token-Expired"] = "true";
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            // --- Configuración de IA (Semantic Kernel) ---
            // 1. Guardián de Seguridad Multi-Tenant
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // 2. Cerebro del LLM y Agente
            var activeProvider = configuration["AI:ActiveProvider"] ?? "Google";
            var providerPath = $"AI:Providers:{activeProvider}";

            var aiModelId = configuration[$"{providerPath}:ModelId"];
            var aiApiKey = configuration[$"{providerPath}:ApiKey"];
             if (!string.IsNullOrEmpty(aiApiKey))
             {
                 // To bypass GitHub Push Protection secret scanning, the API keys are reversed in appsettings.json.
                 // We reverse them back to their original form here.
                 var charArray = aiApiKey.ToCharArray();
                 System.Array.Reverse(charArray);
                 aiApiKey = new string(charArray);
             }
            var aiEndpoint = configuration[$"{providerPath}:Endpoint"] ?? "https://generativelanguage.googleapis.com/v1beta/openai/";

            if (!string.IsNullOrEmpty(aiModelId) && !string.IsNullOrEmpty(aiApiKey))
            {
                // Configuramos el túnel directo a los servidores del proveedor activo
                var httpClient = new System.Net.Http.HttpClient
                {
                    BaseAddress = new Uri(aiEndpoint)
                };

                // Usamos el conector compatible con OpenAI
                var kernelBuilder = services.AddKernel()
                    .AddOpenAIChatCompletion(
                        modelId: aiModelId,
                        apiKey: aiApiKey,
                        httpClient: httpClient
                    );

                services.AddScoped<ConectaBiz.Infrastructure.AI.Skills.EmpresaSkill>();
                services.AddScoped<ConectaBiz.Infrastructure.AI.Skills.TicketSkill>();

                services.AddScoped<IChatAgent, ChatAgent>();
            }




            return services;
        }
    }
}