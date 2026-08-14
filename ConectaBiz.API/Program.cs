// ConectaBiz.API/Program.cs
using ConectaBiz.API.Middleware;
using ConectaBiz.Infrastructure;
using Microsoft.OpenApi.Models;
using ConectaBiz.API.Jobs;
using ConectaBiz.Application;
using ConectaBiz.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.RateLimiting;


var builder = WebApplication.CreateBuilder(args);

// Configuración de CORS dinámico por ambiente
var origenesPermitidos = builder.Configuration
    .GetSection("ConfiguracionCors:OrigenesPermitidos")
    .Get<string[]>() ?? new[] { "http://localhost:3006" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(origenesPermitidos)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("Authorization", "X-Token-Expired");
    });
});

// Configuración de URL y Puerto del servidor dinámico por ambiente
var urlServidor = builder.Configuration["UrlServidor"];
if (!string.IsNullOrWhiteSpace(urlServidor))
{
    builder.WebHost.UseUrls(urlServidor);
}

// Add services to the container
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

// Add Infrastructure Layer
builder.Services.AddInfrastructure(builder.Configuration);

// Add Application Layer
builder.Services.AddApplicationLayer();

// Health Checks
builder.Services.AddHealthChecks();

// Add API Layer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ConectaBiz API", Version = "v1" });

    // Configuración de Swagger para JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHostedService<ConectaBiz.API.Jobs.RecurringJobWorker>();
builder.Services.Configure<ReportesPorCorreoJobSettings>(
    builder.Configuration.GetSection("ReportesPorCorreoJob")
);

builder.Services.AddHostedService<ReportesPorCorreoWorker>();

builder.Services.Configure<WhatsAppJobSettings>(
    builder.Configuration.GetSection("WhatsAppJobSettings")
);
builder.Services.AddHostedService<WhatsAppNotificationWorker>();

// PATRÓN IMPLEMENTADO: Rate Limiting (Limitación de Tasa) / Security
// 
// Protege la API contra ataques de fuerza bruta (ej. múltiples intentos de login)
// o ataques de denegación de servicio (DDoS) limitando la cantidad de peticiones.
// Aquí configuramos la política "auth-strict" (usada en AuthController) que
// rechaza las peticiones con un código 429 (Too Many Requests) si exceden
// el límite de 5 intentos por cada ventana de 1 minuto.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("auth-strict", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (builder.Configuration.GetValue<bool>("HabilitarSwagger", true))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ConectaBiz API V1");
        c.RoutePrefix = "swagger"; // Swagger estará en /swagger
    });
}

// Global error handling middleware
app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseCors("AllowFrontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseResponseCompression();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

app.Run();