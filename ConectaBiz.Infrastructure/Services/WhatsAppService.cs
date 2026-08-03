using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConectaBiz.Infrastructure.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WhatsAppService> _logger;
        private readonly string _url;
        private readonly string _apiKey;
        private readonly string _defaultRemitente;

        public WhatsAppService(HttpClient httpClient, IConfiguration configuration, ILogger<WhatsAppService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _url = configuration["WhatsAppSettings:Url"] ?? throw new ArgumentNullException("WhatsAppSettings:Url is not configured");
            _apiKey = configuration["WhatsAppSettings:ApiKey"] ?? throw new ArgumentNullException("WhatsAppSettings:ApiKey is not configured");
            _defaultRemitente = configuration["WhatsAppSettings:Remitente"] ?? string.Empty;
        }

        public async Task<bool> EnviarWhatsAppAsync(EnviarWhatsAppDto dto)
        {
            try
            {
                var payload = new
                {
                    remitente = !string.IsNullOrWhiteSpace(dto.Remitente) ? dto.Remitente : _defaultRemitente,
                    telefonos = dto.Telefonos,
                    mensaje = dto.Mensaje
                };

                using var request = new HttpRequestMessage(HttpMethod.Post, _url);
                request.Headers.Add("X-API-Key", _apiKey);
                request.Content = JsonContent.Create(payload);

                _logger.LogInformation("Enviando WhatsApp a los teléfonos: {Telefonos}", string.Join(", ", dto.Telefonos));

                using var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("WhatsApp enviado correctamente.");
                    return true;
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al enviar WhatsApp. Código de estado: {StatusCode}, Respuesta: {Response}", response.StatusCode, errorResponse);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción ocurrida al intentar enviar WhatsApp.");
                return false;
            }
        }
    }
}
