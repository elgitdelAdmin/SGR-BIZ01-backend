using ConectaBiz.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace ConectaBiz.Application.Services
{
    public class EmailService: IEmailService
    {
        private readonly string _remitente;
        private readonly string _password;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _rutaLog;

        public EmailService(IConfiguration configuration)
        {
            // Leer directamente del appsettings.json
            _remitente = configuration["CorreoSettings:remitente"];
            _password = configuration["CorreoSettings:password"];
            _smtpServer = configuration["CorreoSettings:smtpServer"];
            _smtpPort = int.Parse(configuration["CorreoSettings:smtpPort"]);
            _rutaLog = configuration["Logging:LogFilePath"];
        }

        /// <summary>
        /// Envía un correo individual a varios destinatarios de manera paralela (actual: secuencial).
        /// </summary>
        public async Task EnviarCorreosAsync(IEnumerable<string> destinatarios, string asunto, string mensajeTexto)
        {
            var swTotal = System.Diagnostics.Stopwatch.StartNew();
            var log = new StringBuilder();
            log.AppendLine("========== INICIO EnviarCorreosAsync ==========");
            log.AppendLine($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            try
            {
                // Normaliza destinatarios
                var to = destinatarios?
                    .Where(d => !string.IsNullOrWhiteSpace(d))
                    .Select(d => d.Trim())
                    .Distinct()
                    .ToList() ?? new List<string>();

                log.AppendLine($"Destinatarios count={to.Count}");
                if (to.Count == 0)
                {
                    log.AppendLine("Sin destinatarios. Saliendo.");
                    return;
                }

                // Construir HTML UNA vez
                var cuerpoHtml = $@"
                    <html>
                      <body style='background-color:#f0f4f8; display:flex; justify-content:center; align-items:center; height:100vh; margin:0;'>
                        <div style='background-color:#4a90e2; color:white; padding:40px 60px; border-radius:12px; text-align:center; font-size:24px; font-weight:bold;'>
                          {mensajeTexto}
                        </div>
                      </body>
                    </html>";

                // Crear el mensaje único
                using (var mensaje = new MailMessage())
                {
                    mensaje.From = new MailAddress(_remitente, "Soporte TI");

                    // Usa BCC para ocultar direcciones entre sí
                    foreach (var email in to)
                    {
                        mensaje.Bcc.Add(new MailAddress(email));
                    }

                    mensaje.Subject = asunto;
                    mensaje.Body = cuerpoHtml;
                    mensaje.IsBodyHtml = true;

                    using (var smtp = new SmtpClient(_smtpServer, _smtpPort))
                    {
                        smtp.Credentials = new NetworkCredential(_remitente, _password);
                        smtp.EnableSsl = true;

                        var t0 = swTotal.ElapsedMilliseconds;
                        await smtp.SendMailAsync(mensaje);
                        log.AppendLine($"SendMailAsync (todos juntos) ms={swTotal.ElapsedMilliseconds - t0}");
                    }
                }
                log.AppendLine($"✅ FIN EnviarCorreosAsync (total ms={swTotal.ElapsedMilliseconds})");
                await File.AppendAllTextAsync(_rutaLog, log.ToString());
            }
            catch (Exception ex)
            {
                log.AppendLine("❌ ERROR EnviarCorreosAsync: " + ex.Message);
                log.AppendLine(ex.StackTrace);
                throw new Exception($"Error al enviar correos: {ex.Message}", ex);
            }
            finally
            {
                log.AppendLine("========== FIN EnviarCorreosAsync ==========");
                //await File.AppendAllTextAsync(_rutaLog, log.ToString());
            }
        }



    }

}
