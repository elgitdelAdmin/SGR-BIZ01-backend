using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ConectaBiz.Application.Services
{
    public class NotificacionSistemaService : INotificacionSistemaService
    {
        private readonly INotificacionSistemaRepository _notificacionRepository;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificacionSistemaService> _logger;

        public NotificacionSistemaService(
            INotificacionSistemaRepository notificacionRepository,
            IWhatsAppService whatsAppService,
            IEmailService emailService,
            ILogger<NotificacionSistemaService> logger)
        {
            _notificacionRepository = notificacionRepository;
            _whatsAppService = whatsAppService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task EnviarAsync(NotificacionSistemaDto request)
        {
            var canalesUtilizados = new List<string>();

            // 1. WhatsApp
            if (request.TelefonosWhatsApp != null && request.TelefonosWhatsApp.Any() && !string.IsNullOrWhiteSpace(request.MensajeWhatsApp))
            {
                try
                {
                    var dto = new EnviarWhatsAppDto
                    {
                        Telefonos = request.TelefonosWhatsApp.Select(t => t.Trim().StartsWith("51") ? t.Trim() : "51" + t.Trim()).ToList(),
                        Mensaje = request.MensajeWhatsApp
                    };
                    var success = await _whatsAppService.EnviarWhatsAppAsync(dto);
                    if (success)
                    {
                        canalesUtilizados.Add("WhatsApp");
                    }
                    else
                    {
                        _logger.LogError("Fallo al enviar WhatsApp en NotificacionSistemaService.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error crítico al enviar WhatsApp en NotificacionSistemaService.");
                }
            }

            // 2. Correo
            if (request.CorreosDestino != null && request.CorreosDestino.Any() && !string.IsNullOrWhiteSpace(request.MensajeCorreoHtml))
            {
                try
                {
                    await _emailService.EnviarCorreosAsync(request.CorreosDestino, request.AsuntoCorreo ?? "Notificación", request.MensajeCorreoHtml);
                    canalesUtilizados.Add("Correo");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar correo en NotificacionSistemaService.");
                }
            }

            // 3. Base de Datos
            if (!string.IsNullOrWhiteSpace(request.MensajeBD))
            {
                try
                {
                    var notificacion = new NotificacionSistema
                    {
                        IdUser = request.IdUser,
                        TipoNotificacion = request.TipoNotificacion,
                        IdReferencia = request.IdReferencia,
                        RutaFrontend = request.RutaFrontend,
                        Mensaje = request.MensajeBD,
                        CanalesEnviados = canalesUtilizados.Any() ? string.Join(",", canalesUtilizados) : null,
                        FechaCreacion = DateTime.UtcNow,
                        Leido = false,
                        Activo = true
                    };
                    
                    await _notificacionRepository.AddAsync(notificacion);
                    canalesUtilizados.Add("BD");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al guardar NotificacionSistema en BD.");
                }
            }

            _logger.LogInformation("Orquestación de NotificacionSistema finalizada. Canales ejecutados exitosamente: {Canales}", string.Join(", ", canalesUtilizados));
        }

        public async Task EnviarLoteAsync(IEnumerable<NotificacionSistemaDto> requests)
        {
            var notificacionesDb = new List<NotificacionSistema>();

            foreach (var request in requests)
            {
                var canalesUtilizados = new List<string>();

                // 1. WhatsApp
                if (request.TelefonosWhatsApp != null && request.TelefonosWhatsApp.Any() && !string.IsNullOrWhiteSpace(request.MensajeWhatsApp))
                {
                    try
                    {
                        var dto = new EnviarWhatsAppDto
                        {
                            Telefonos = request.TelefonosWhatsApp.Select(t => t.Trim().StartsWith("51") ? t.Trim() : "51" + t.Trim()).ToList(),
                            Mensaje = request.MensajeWhatsApp
                        };
                        var success = await _whatsAppService.EnviarWhatsAppAsync(dto);
                        if (success) canalesUtilizados.Add("WhatsApp");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al enviar WhatsApp en lote.");
                    }
                }

                // 2. Correo
                if (request.CorreosDestino != null && request.CorreosDestino.Any() && !string.IsNullOrWhiteSpace(request.MensajeCorreoHtml))
                {
                    try
                    {
                        await _emailService.EnviarCorreosAsync(request.CorreosDestino, request.AsuntoCorreo ?? "Notificación", request.MensajeCorreoHtml);
                        canalesUtilizados.Add("Correo");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al enviar correo en lote.");
                    }
                }

                // 3. Preparar Entidad BD
                if (!string.IsNullOrWhiteSpace(request.MensajeBD))
                {
                    canalesUtilizados.Add("BD");
                    var notificacion = new NotificacionSistema
                    {
                        IdUser = request.IdUser,
                        TipoNotificacion = request.TipoNotificacion,
                        IdReferencia = request.IdReferencia,
                        RutaFrontend = request.RutaFrontend,
                        Mensaje = request.MensajeBD,
                        CanalesEnviados = canalesUtilizados.Any() ? string.Join(",", canalesUtilizados) : null,
                        FechaCreacion = DateTime.UtcNow,
                        Leido = false,
                        Activo = true
                    };
                    notificacionesDb.Add(notificacion);
                }
            }

            // Inserción por lotes
            if (notificacionesDb.Any())
            {
                try
                {
                    await _notificacionRepository.AddRangeAsync(notificacionesDb);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al guardar lote de notificaciones en BD.");
                }
            }

            _logger.LogInformation("Orquestación de lote finalizada. Notificaciones BD generadas: {Count}", notificacionesDb.Count);
        }

        public async Task<IEnumerable<NotificacionSistema>> ObtenerTodasPorUsuarioAsync(int idUser)
        {
            return await _notificacionRepository.GetNotificacionesByUserIdAsync(idUser);
        }

        public async Task<IEnumerable<NotificacionSistema>> ObtenerPorReferenciaYUsuariosAsync(int idReferencia, int[] idUsers)
        {
            return await _notificacionRepository.GetByReferenciaAndUsersAsync(idReferencia, idUsers);
        }

        public async Task<IEnumerable<NotificacionSistema>> ObtenerNoLeidasPorUsuarioAsync(int idUser)
        {
            return await _notificacionRepository.GetNotificacionesNoLeidasByUserIdAsync(idUser);
        }

        public async Task MarcarComoLeidaAsync(int idUser, List<int> idsNotificaciones)
        {
            await _notificacionRepository.MarcarComoLeidaAsync(idUser, idsNotificaciones);
        }

        public async Task EliminarNotificacionAsync(int id)
        {
            await _notificacionRepository.EliminarLogicaAsync(id);
        }
    }
}
