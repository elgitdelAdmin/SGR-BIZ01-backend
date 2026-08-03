using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Constants;
using ConectaBiz.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ConectaBiz.Application.Services
{
    public class NotificacionWhatsAppService : INotificacionWhatsAppService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;
        private readonly IParametrosCatalogo _parametrosCatalogo;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IOptions<WhatsAppJobSettings> _jobSettings;
        private readonly ILogger<NotificacionWhatsAppService> _logger;

        private static readonly HashSet<string> _historialEnviosGestoresCuenta = new HashSet<string>();
        private static readonly HashSet<string> _historialEnviosConsultores = new HashSet<string>();

        public NotificacionWhatsAppService(
            ITicketRepository ticketRepository,
            IUserRepository userRepository,
            IParametrosCatalogo parametrosCatalogo,
            IWhatsAppService whatsAppService,
            IOptions<WhatsAppJobSettings> jobSettings,
            ILogger<NotificacionWhatsAppService> logger)
        {
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _parametrosCatalogo = parametrosCatalogo;
            _whatsAppService = whatsAppService;
            _jobSettings = jobSettings;
            _logger = logger;
        }

        public async Task EnviarNotificacionesWhatsAppAsync()
        {
            _logger.LogInformation("Job ejecutó EnviarNotificacionesWhatsAppAsync periódicamente.");

            try
            {
                // Cargar catálogo de parámetros
                await _parametrosCatalogo.EnsureLoadedAsync();
                
                // Obtener el ID del estado PENDIENTE_ATENCION
                var estadoPendiente = _parametrosCatalogo.Current.ListaEstados
                    .FirstOrDefault(e => e.Codigo == AppConstants.Estados.PENDIENTE_ATENCION);

                if (estadoPendiente == null)
                {
                    _logger.LogWarning("No se encontró el estado PENDIENTE_ATENCION en los parámetros.");
                    return;
                }

                // Verificar horarios específicos antes de consultar a la base de datos
                var horasCuenta = _jobSettings.Value.HorasEnvioGestoresCuenta;
                var horasConsultores = _jobSettings.Value.HorasEnvioGestoresConsultoria;

                var ejecutarCuenta = DebeEjecutar(horasCuenta, _historialEnviosGestoresCuenta, out var horaCuentaAEjecutar);
                var ejecutarConsultores = DebeEjecutar(horasConsultores, _historialEnviosConsultores, out var horaConsultoresAEjecutar);

                if (!ejecutarCuenta && !ejecutarConsultores)
                {
                    _logger.LogInformation("No hay envíos programados para ejecutar en este ciclo.");
                    return;
                }

                // Obtener IDs de los estados a excluir
                var estadosAExcluir = new[]
                {
                    AppConstants.Estados.CERRADO,
                    AppConstants.Estados.CANCELADO,
                    AppConstants.Estados.RECHAZADO,
                    AppConstants.Estados.ANULADO
                };
                var idsEstadosAExcluir = _parametrosCatalogo.Current.ListaEstados
                    .Where(e => estadosAExcluir.Contains(e.Codigo))
                    .Select(e => e.Id)
                    .ToList();

                var hoy = DateTime.Now;
                var haceUnMes = hoy.AddMonths(-1);

                _logger.LogInformation("Consultando tickets desde {Desde} hasta {Hasta}...", haceUnMes, hoy);

                // Consultar todos los tickets del último mes que estén activos, excluyendo los estados CERRADO, CANCELADO, RECHAZADO, ANULADO
                var todosLosTickets = await _ticketRepository.GetQueryableAll()
                    .Include(t => t.FrenteSubFrentes)
                    .Where(t => t.Activo 
                             && t.FechaSolicitud >= haceUnMes 
                             && t.FechaSolicitud <= hoy
                             && !idsEstadosAExcluir.Contains(t.IdEstadoTicket))
                    .ToListAsync();

                // Separar en memoria
                var ticketsPendientes = todosLosTickets
                    .Where(t => t.IdEstadoTicket == estadoPendiente.Id)
                    .ToList();

                var ticketsConFrenteVencido = todosLosTickets
                    .Where(t => t.FrenteSubFrentes.Any(fs => fs.Activo && fs.FechaFin < hoy))
                    .ToList();

                _logger.LogInformation("Se encontraron {Count} tickets pendientes de atención.", ticketsPendientes.Count);
                _logger.LogInformation("Se encontraron {Count} tickets con frentes vencidos.", ticketsConFrenteVencido.Count);

                if (ticketsPendientes.Count == 0 && ticketsConFrenteVencido.Count == 0)
                {
                    // Si no hay tickets de ningún tipo, igual registramos la ejecución para evitar reintentos continuos hoy
                    if (ejecutarCuenta) RegistrarEjecucion(horaCuentaAEjecutar, _historialEnviosGestoresCuenta);
                    if (ejecutarConsultores) RegistrarEjecucion(horaConsultoresAEjecutar, _historialEnviosConsultores);
                    return;
                }

                if (ejecutarCuenta)
                {
                    await NotificarGestoresCuentaAsync(ticketsPendientes);
                    RegistrarEjecucion(horaCuentaAEjecutar, _historialEnviosGestoresCuenta);
                }

                if (ejecutarConsultores)
                {
                    await NotificarGestoresConsultorialAsync(ticketsPendientes, ticketsConFrenteVencido);
                    RegistrarEjecucion(horaConsultoresAEjecutar, _historialEnviosConsultores);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar y enviar notificaciones de WhatsApp en EnviarNotificacionesWhatsAppAsync.");
            }
        }

        private async Task NotificarGestoresCuentaAsync(List<ConectaBiz.Domain.Entities.Ticket> ticketsPendientes)
        {
            _logger.LogInformation("Iniciando FASE 1: Notificar a cada Gestor de Cuenta...");

            // Mapear cada ticket a sus gestores activos (vía EmpresaGestores o fallback a Gestor directo)
            var ticketsPorGestor = new Dictionary<ConectaBiz.Domain.Entities.Gestor, List<ConectaBiz.Domain.Entities.Ticket>>();

            foreach (var ticket in ticketsPendientes)
            {
                if (ticket.Empresa == null) continue;

                var gestoresActivos = ticket.Empresa.EmpresaGestores != null && ticket.Empresa.EmpresaGestores.Any(eg => eg.Activo)
                    ? ticket.Empresa.EmpresaGestores.Where(eg => eg.Activo && eg.Gestor != null).Select(eg => eg.Gestor).ToList()
                    : (ticket.Empresa.Gestor != null ? new List<ConectaBiz.Domain.Entities.Gestor> { ticket.Empresa.Gestor } : new List<ConectaBiz.Domain.Entities.Gestor>());

                foreach (var gestor in gestoresActivos)
                {
                    if (!ticketsPorGestor.ContainsKey(gestor))
                    {
                        ticketsPorGestor[gestor] = new List<ConectaBiz.Domain.Entities.Ticket>();
                    }
                    if (!ticketsPorGestor[gestor].Contains(ticket))
                    {
                        ticketsPorGestor[gestor].Add(ticket);
                    }
                }
            }

            foreach (var kvp in ticketsPorGestor)
            {
                var gestor = kvp.Key;
                var grupo = kvp.Value;
                var persona = gestor.Persona;

                var nombreGestor = persona != null
                    ? $"{persona.Nombres} {persona.ApellidoPaterno}".Trim()
                    : "Gestor";

                var telefono = persona?.Telefono?.Trim();

                if (string.IsNullOrWhiteSpace(telefono))
                {
                    _logger.LogWarning("El gestor de cuenta {Nombre} (ID {Id}) no tiene un teléfono configurado.", nombreGestor, gestor.Id);
                    continue;
                }

                // Asegurar que el número comience con el prefijo "51"
                if (!telefono.StartsWith("51"))
                {
                    telefono = "51" + telefono;
                }

                // Armar lista de códigos de tickets (incluyendo código interno si existe)
                var lineasTickets = grupo.Select(t => 
                    string.IsNullOrWhiteSpace(t.CodTicketInterno) 
                        ? t.CodTicket 
                        : $"{t.CodTicket} / {t.CodTicketInterno.Trim()}"
                ).ToList();
                var codigosString = "- " + string.Join("\n- ", lineasTickets);

                // Formatear mensaje
                var mensaje = $"¡Hola {nombreGestor}! 👋 Tienes algunos tickets pendientes de atención esperándote:\n\n{codigosString}\n\n¡Échales un vistazo cuando puedas! 🚀\n\n🤖 *Soy el asistente automático de Conecta.* Por favor, no me respondas por aquí, recuerda actualizar tus tickets directamente en el sistema. ¡Gracias!";

                // Configurar el DTO de envío
                var dto = new EnviarWhatsAppDto
                {
                    Telefonos = new List<string> { telefono },
                    Mensaje = mensaje
                };

                _logger.LogInformation("Enviando notificación de WhatsApp al Gestor de Cuenta {Nombre} ({Telefono})...", nombreGestor, telefono);
                var success = await _whatsAppService.EnviarWhatsAppAsync(dto);

                if (success)
                {
                    _logger.LogInformation("Notificación enviada exitosamente al Gestor de Cuenta {Nombre}.", nombreGestor);
                }
                else
                {
                    _logger.LogError("Fallo al enviar notificación de WhatsApp al Gestor de Cuenta {Nombre}.", nombreGestor);
                }
            }
        }

        private async Task NotificarGestoresConsultorialAsync(
            List<ConectaBiz.Domain.Entities.Ticket> ticketsPendientes,
            List<ConectaBiz.Domain.Entities.Ticket> ticketsConFrenteVencido)
        {
            _logger.LogInformation("Iniciando FASE 2: Notificar a Gestores de Consultoría y Teléfono Adicional...");

            // Obtener teléfono adicional desde settings
            var telefonoAdicional = _jobSettings.Value.TelefonoAdicional?.Trim();

            // Buscar usuarios con rol GESTORCONSULTORIA
            var usuarios = await _userRepository.GetAllAsync();
            var gestoresConsultoria = usuarios
                .Where(u => u.UserRolSocios.Any(urs => urs.Rol != null && urs.Rol.Codigo == AppConstants.Roles.GestorConsultoria && urs.Activo)
                         && u.Persona != null)
                .ToList();

            var tieneGestores = gestoresConsultoria.Count > 0;
            var tieneTelefonoAdicional = !string.IsNullOrWhiteSpace(telefonoAdicional);

            if (tieneGestores || tieneTelefonoAdicional)
            {
                _logger.LogInformation("Preparando reporte consolidado para {Count} Gestores de Consultoría y Teléfono Adicional...", gestoresConsultoria.Count);

                // Armar lista de tickets con su respectivo gestor de cuenta asignado (incluyendo código interno si existe)
                var lineasTickets = ticketsPendientes.Select(FormatearLineaTicket).ToList();
                var listadoTicketsString = "- " + string.Join("\n- ", lineasTickets);

                // Armar lista para tickets con frentes activos/planificados en curso (vencidos)
                var hoy = DateTime.Now;
                var lineasFrenteVencido = ticketsConFrenteVencido.Select(t => FormatearLineaTicketConFechas(t, hoy)).ToList();
                var listadoFrentesVencidosString = lineasFrenteVencido.Count > 0
                    ? "- " + string.Join("\n- ", lineasFrenteVencido)
                    : null;

                // 1. Enviar a cada Gestor de Consultoría
                foreach (var gc in gestoresConsultoria)
                {
                    var persona = gc.Persona;
                    var nombreGc = $"{persona.Nombres} {persona.ApellidoPaterno}".Trim();
                    var telefono = persona.Telefono?.Trim();

                    if (string.IsNullOrWhiteSpace(telefono))
                    {
                        _logger.LogWarning("El Gestor de Consultoría {Nombre} no tiene un teléfono configurado.", nombreGc);
                        continue;
                    }

                    // Asegurar que el número comience con el prefijo "51"
                    if (!telefono.StartsWith("51"))
                    {
                        telefono = "51" + telefono;
                    }

                    // Formatear mensaje consolidado
                    var mensajeConsolidado = $"¡Hola {nombreGc}! 👋 Tienes algunos tickets pendientes de atención esperándote:\n\n{listadoTicketsString}";
                    if (listadoFrentesVencidosString != null)
                    {
                        mensajeConsolidado += $"\n\n*Tienes tickets con asignaciones vencidas que no están cerrados:*\n{listadoFrentesVencidosString}";
                    }
                    mensajeConsolidado += "\n\n¡Échales un vistazo cuando puedas! 🚀\n\n🤖 *Soy el asistente automático de Conecta.* Por favor, no me respondas por aquí, recuerda actualizar tus tickets directamente en el sistema. ¡Gracias!";

                    var dto = new EnviarWhatsAppDto
                    {
                        Telefonos = new List<string> { telefono },
                        Mensaje = mensajeConsolidado
                    };

                    _logger.LogInformation("Enviando reporte consolidado de WhatsApp al Gestor de Consultoría {Nombre} ({Telefono})...", nombreGc, telefono);
                    var success = await _whatsAppService.EnviarWhatsAppAsync(dto);

                    if (success)
                    {
                        _logger.LogInformation("Reporte consolidado enviado exitosamente al Gestor de Consultoría {Nombre}.", nombreGc);
                    }
                    else
                    {
                        _logger.LogError("Fallo al enviar reporte consolidado al Gestor de Consultoría {Nombre}.", nombreGc);
                    }
                }

                // 2. Enviar al Teléfono Adicional configurado (excluyendo a los gestores de cuenta)
                if (tieneTelefonoAdicional)
                {
                    // Asegurar prefijo "51" (removiendo espacios si los tiene)
                    var telefonoLimpio = telefonoAdicional.Replace(" ", "");
                    if (!telefonoLimpio.StartsWith("51"))
                    {
                        telefonoLimpio = "51" + telefonoLimpio;
                    }

                    var mensajeAdicional = $"¡Hola! 👋 Tienes algunos tickets pendientes de atención esperándote:\n\n{listadoTicketsString}";
                    if (listadoFrentesVencidosString != null)
                    {
                        mensajeAdicional += $"\n\n*Tienes tickets con asignaciones vencidas que no están cerrados:*\n{listadoFrentesVencidosString}";
                    }
                    mensajeAdicional += "\n\n¡Échales un vistazo cuando puedas! 🚀\n\n🤖 *Soy el asistente automático de Conecta.* Por favor, no me respondas por aquí, recuerda actualizar tus tickets directamente en el sistema. ¡Gracias!";

                    var dto = new EnviarWhatsAppDto
                    {
                        Telefonos = new List<string> { telefonoLimpio },
                        Mensaje = mensajeAdicional
                    };

                    _logger.LogInformation("Enviando reporte consolidado al Teléfono Adicional ({Telefono})...", telefonoLimpio);
                    var success = await _whatsAppService.EnviarWhatsAppAsync(dto);

                    if (success)
                    {
                        _logger.LogInformation("Reporte consolidado enviado exitosamente al Teléfono Adicional {Telefono}.", telefonoLimpio);
                    }
                    else
                    {
                        _logger.LogError("Fallo al enviar reporte consolidado al Teléfono Adicional {Telefono}.", telefonoLimpio);
                    }
                }
            }
        }

        private bool DebeEjecutar(List<string> horasConfiguradas, HashSet<string> historial, out string horaAEjecutar)
        {
            horaAEjecutar = null;
            if (horasConfiguradas == null || horasConfiguradas.Count == 0)
            {
                return false;
            }

            var ahora = DateTime.Now;
            var hoyString = ahora.ToString("yyyy-MM-dd");

            // Limpiar historial de días anteriores para evitar consumo de memoria acumulado
            historial.RemoveWhere(key => !key.StartsWith(hoyString));

            foreach (var horaStr in horasConfiguradas)
            {
                var trimHora = horaStr?.Trim();
                if (string.IsNullOrWhiteSpace(trimHora) || !System.Text.RegularExpressions.Regex.IsMatch(trimHora, @"^\d{2}:\d{2}$"))
                {
                    continue;
                }

                var parts = trimHora.Split(':');
                var hora = int.Parse(parts[0]);
                var minuto = int.Parse(parts[1]);

                var programado = new DateTime(ahora.Year, ahora.Month, ahora.Day, hora, minuto, 0);
                var limiteMaximo = programado.AddMinutes(10); // Ventana de gracia de 10 minutos para evitar poner al día horarios pasados al iniciar/reiniciar

                if (ahora >= programado && ahora <= limiteMaximo)
                {
                    var claveHistorial = $"{hoyString}_{trimHora}";
                    if (!historial.Contains(claveHistorial))
                    {
                        horaAEjecutar = trimHora;
                        return true;
                    }
                }
            }

            return false;
        }

        private static void RegistrarEjecucion(string horaStr, HashSet<string> historial)
        {
            var hoyString = DateTime.Now.ToString("yyyy-MM-dd");
            var claveHistorial = $"{hoyString}_{horaStr.Trim()}";
            historial.Add(claveHistorial);
        }

        private static string FormatearLineaTicket(ConectaBiz.Domain.Entities.Ticket ticket)
        {
            var gestor = ticket.Empresa?.Gestor?.Persona;
            var nombreGestor = gestor != null
                ? $"{gestor.Nombres} {gestor.ApellidoPaterno}".Trim()
                : "Sin gestor";

            var ticketStr = string.IsNullOrWhiteSpace(ticket.CodTicketInterno)
                ? ticket.CodTicket
                : $"{ticket.CodTicket} / {ticket.CodTicketInterno.Trim()}";

            return $"{ticketStr} (Gestor: {nombreGestor})";
        }

        private static string FormatearLineaTicketConFechas(ConectaBiz.Domain.Entities.Ticket ticket, DateTime hoy)
        {
            var gestor = ticket.Empresa?.Gestor?.Persona;
            var nombreGestor = gestor != null
                ? $"{gestor.Nombres} {gestor.ApellidoPaterno}".Trim()
                : "Sin gestor";

            var ticketStr = string.IsNullOrWhiteSpace(ticket.CodTicketInterno)
                ? ticket.CodTicket
                : $"{ticket.CodTicket} / {ticket.CodTicketInterno.Trim()}";

            var frentesVencidos = ticket.FrenteSubFrentes
                .Where(fs => fs.Activo && fs.FechaFin < hoy)
                .ToList();

            var rangoFechas = "";
            if (frentesVencidos.Count > 0)
            {
                var minInicio = frentesVencidos.Min(fs => fs.FechaInicio);
                var maxFin = frentesVencidos.Max(fs => fs.FechaFin);
                rangoFechas = $" [Inicio: {minInicio:dd/MM/yyyy} - Fin: {maxFin:dd/MM/yyyy}]";
            }

            return $"{ticketStr} (Gestor: {nombreGestor}){rangoFechas}";
        }
    }
}
