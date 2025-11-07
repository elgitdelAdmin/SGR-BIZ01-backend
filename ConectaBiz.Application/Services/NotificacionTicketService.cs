using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Services
{
    public class NotificacionTicketService : INotificacionTicketService
    {
        private readonly INotificacionTicketRepository _repository;
        private readonly IAuthService _userService;
        private readonly IEmailService _emailService;
        private readonly string _rutaLog;

        public NotificacionTicketService(INotificacionTicketRepository repository, IAuthService userService, IEmailService emailService, IConfiguration configuration)
        {
            _repository = repository;
            _userService = userService;
            _emailService = emailService;
            _rutaLog = configuration["Logging:LogFilePath"];
        }

        public async Task<IEnumerable<NotificacionTicketDto>> GetNotificacionesByUserIdAsync(int idUser)
        {
            var notificaciones = await _repository.GetNotificacionesByUserIdAsync(idUser);

            return notificaciones.Select(n => new NotificacionTicketDto
            {
                Id = n.Id,
                IdTicket = n.IdTicket,
                Mensaje = n.Mensaje,
                Leido = n.Leido,
                FechaCreacion = n.FechaCreacion,
                FechaLectura = n.FechaLectura
            });
        }

        public async Task<IEnumerable<NotificacionTicketDto>> GetNotificacionesNoLeidasByUserIdAsync(int idUser)
        {
            var notificaciones = await _repository.GetNotificacionesNoLeidasByUserIdAsync(idUser);

            return notificaciones.Select(n => new NotificacionTicketDto
            {
                Id = n.Id,
                IdTicket = n.IdTicket,
                Mensaje = n.Ticket?.CodTicket ?? "",
                Leido = n.Leido,
                FechaCreacion = n.FechaCreacion,
                FechaLectura = n.FechaLectura
            });
        }
        public async Task<IEnumerable<NotificacionTicketDto>> GetNotificacionesByIdTicketAsync(int idTicket)
        {
            var notificaciones = await _repository.GetNotificacionesByIdTicketAsync(idTicket);

            return notificaciones.Select(n => new NotificacionTicketDto
            {
                Id = n.Id,
                IdTicket = n.IdTicket,
                Mensaje = n.Ticket?.CodTicket ?? "",
                Leido = n.Leido,
                FechaCreacion = n.FechaCreacion,
                FechaLectura = n.FechaLectura
            });
        }
        public async Task<IEnumerable<NotificacionTicketDto>> GetNotificacionesByIdTicketIdUserAsync(int idTicket, int idUser)
        {
            var notificaciones = await _repository.GetNotificacionesByIdTicketIdUserAsync(idTicket, idUser);

            return notificaciones.Select(n => new NotificacionTicketDto
            {
                Id = n.Id,
                IdTicket = n.IdTicket,
                Mensaje = n.Ticket?.CodTicket ?? "",
                Leido = n.Leido,
                FechaCreacion = n.FechaCreacion,
                FechaLectura = n.FechaLectura
            });
        }
        public async Task<IEnumerable<NotificacionTicketDto>> GetNotificacionesByIdTicketIdUsersAsync(int idTicket, int[] idUsers)
        {
            var notificaciones = await _repository.GetNotificacionesByIdTicketIdUsersAsync(idTicket, idUsers);

            return notificaciones.Select(n => new NotificacionTicketDto
            {
                Id = n.Id,
                IdTicket = n.IdTicket,
                IdUser = n.IdUser,
                Mensaje = n.Ticket?.CodTicket ?? "",
                Leido = n.Leido,
                FechaCreacion = n.FechaCreacion,
                FechaLectura = n.FechaLectura
            });
        }

        public async Task<bool> MarcarComoLeidaAsync(int idUser, int[] idsNotificaciones)
        {
            return await _repository.MarcarComoLeidaAsync(idUser, idsNotificaciones);
        }
        //public async Task<bool> MarcarTodasComoLeidasAsync(int idUser)
        //{
        //    return await _repository.MarcarTodasComoLeidasAsync(idUser);
        //}
        public async Task<bool> DesactivarAsync(int idUser, int[] idsNotificaciones)
        {
            return await _repository.DesactivarAsync(idUser, idsNotificaciones);
        }

        public async Task<int> ContarNoLeidasAsync(int idUser)
        {
            return await _repository.ContarNoLeidasAsync(idUser);
        }
        public async Task<NotificacionTicket> AddAsync(CrearNotificacionDto dto)
        {
            var notificacion = new NotificacionTicket
            {
                IdTicket = dto.IdTicket,
                IdUser = dto.IdUser,
                Leido = false,
                FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
                Activo = true
            };

            return await _repository.AddAsync(notificacion);
        }

        public async Task<IEnumerable<NotificacionTicket>> AddRangeAsync(IEnumerable<CrearNotificacionDto> dtos)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var log = new StringBuilder();
            log.AppendLine("========== INICIO AddRangeAsync ==========");
            log.AppendLine($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            long t0;

            try
            {
                // 1️⃣ Obtener todos los usuarios
                t0 = sw.ElapsedMilliseconds;
                var users = await _userService.GetUsersByIdAsync(dtos.Select(d => d.IdUser).ToArray());
                log.AppendLine($"GetUsersByIdAsync ms={sw.ElapsedMilliseconds - t0}");

                // 2️⃣ Filtrar correos válidos
                t0 = sw.ElapsedMilliseconds;
                var destinatarios = users
                    .Where(u => !string.IsNullOrEmpty(u.Persona?.Correo))
                    .Select(u => u.Persona.Correo!.Trim())
                    .Distinct()
                    .ToList();
                log.AppendLine($"Filtrar destinatarios ms={sw.ElapsedMilliseconds - t0} (count={destinatarios.Count})");

                // 3️⃣ Mensaje global
                var mensajeGlobal = dtos.FirstOrDefault()?.Mensaje ?? "Hola";
                log.AppendLine("Mensaje global preparado.");

                // 4️⃣ Enviar correos
                t0 = sw.ElapsedMilliseconds;
                //await _emailService.EnviarCorreosAsync(destinatarios, "ConectaBiz", mensajeGlobal);
                log.AppendLine($"EnviarCorreosAsync ms={sw.ElapsedMilliseconds - t0}");

                // 5️⃣ Crear notificaciones
                t0 = sw.ElapsedMilliseconds;
                var notificaciones = dtos.Select(dto => new NotificacionTicket
                {
                    IdTicket = dto.IdTicket,
                    IdUser = dto.IdUser,
                    Mensaje = dto.Mensaje,
                    Leido = false,
                    FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
                    Activo = true
                }).ToList();
                log.AppendLine($"Construcción notificaciones ms={sw.ElapsedMilliseconds - t0} (count={notificaciones.Count})");

                // 6️⃣ Guardar en repo
                t0 = sw.ElapsedMilliseconds;
                var resultado = await _repository.AddRangeAsync(notificaciones);
                log.AppendLine($"Repository.AddRangeAsync ms={sw.ElapsedMilliseconds - t0}");

                log.AppendLine($"✅ FIN EXITOSO (total ms={sw.ElapsedMilliseconds})");
                //await File.AppendAllTextAsync(_rutaLog, log.ToString());

                return resultado;
            }
            catch (Exception ex)
            {
                log.AppendLine("❌ ERROR: " + ex.Message);
                log.AppendLine(ex.StackTrace);
                throw;
            }
            finally
            {
                log.AppendLine("========== FIN AddRangeAsync ==========");
                await File.AppendAllTextAsync("log_addRange.txt", log.ToString());
            }
        }

    }
}
