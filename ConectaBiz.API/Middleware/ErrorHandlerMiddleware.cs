// ConectaBiz.API/Middleware/ErrorHandlerMiddleware.cs
using System.Net;
using System.Text.Json;

namespace ConectaBiz.API.Middleware
{
    // PATRÓN IMPLEMENTADO: Global Exception Handler (Manejo Global de Excepciones) / Middleware
    // 
    // Centraliza el manejo de excepciones de toda la aplicación en un solo punto.
    // Atrapa los errores que suben desde cualquier capa (Dominio, Aplicación, Controladores)
    // y los mapea automáticamente a códigos de estado HTTP estándar (400, 404, 409, 500),
    // asegurando una estructura de respuesta unificada ({ message: "..." }) para el frontend.
    // Esto evita esparcir bloques try-catch por todos los controladores.
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                // Mapeo de Excepciones a Status Codes
                response.StatusCode = error switch
                {
                    ConectaBiz.Application.Exceptions.ConsultoresAsociadosException => (int)HttpStatusCode.Conflict,
                    UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                    InvalidOperationException => (int)HttpStatusCode.BadRequest,
                    ArgumentException => (int)HttpStatusCode.BadRequest,
                    KeyNotFoundException => (int)HttpStatusCode.NotFound,
                    FileNotFoundException => (int)HttpStatusCode.NotFound,
                    _ => (int)HttpStatusCode.InternalServerError,
                };

                // Loggear siempre los detalles completos para el servidor
                if (response.StatusCode == (int)HttpStatusCode.InternalServerError)
                {
                    _logger.LogError(error, "Unhandled exception en {Path}", context.Request.Path);
                }
                else
                {
                    _logger.LogWarning(error, "Handled exception {Type} en {Path}", error.GetType().Name, context.Request.Path);
                }

                // Determinar el payload para el cliente
                object payload = error switch
                {
                    ConectaBiz.Application.Exceptions.ConsultoresAsociadosException cae => new { message = cae.Message, consultores = cae.Consultores },
                    _ when response.StatusCode < 500 => new { message = error.Message }, // Errores de negocio: se expone el mensaje
                    _ => new { message = "Error interno del servidor" } // Error 500: se oculta el mensaje real
                };

                var result = JsonSerializer.Serialize(payload);
                await response.WriteAsync(result);
            }
        }
    }
}