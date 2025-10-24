// ConectaBiz.API/Program.cs
using ConectaBiz.API.Middleware;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Application.Mappings;
using ConectaBiz.Application.Services;
using ConectaBiz.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ConectaBiz.Infrastructure.Persistence.Contexts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3006", "http://154.38.177.31:3000") // Tu frontend React
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Forzar escuchar en IPv4 espec�ficamente
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// Add services to the container
builder.Services.AddControllers();

// Add Infrastructure Layer
builder.Services.AddInfrastructure(builder.Configuration);

// Add Application Layer
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<SGRCSTIService>();



// Add API Layer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ConectaBiz API", Version = "v1" });

    // Configuraci�n de Swagger para JWT
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

builder.Services.AddHostedService<MigracionRequerimientosWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ConectaBiz API V1");
    c.RoutePrefix = "swagger"; // Swagger estar� en /swagger
});


// Global error handling middleware
app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowFrontend");

app.MapControllers();

app.Run();