using System.Collections.Generic;

namespace ConectaBiz.Application.DTOs
{
    public class NotificacionSistemaDto
    {
        public int? IdUser { get; set; }
        public string TipoNotificacion { get; set; }
        public int? IdReferencia { get; set; }
        public string RutaFrontend { get; set; }

        // Opciones para Base de Datos
        public string MensajeBD { get; set; }

        // Opciones para WhatsApp
        public List<string> TelefonosWhatsApp { get; set; } = new List<string>();
        public string MensajeWhatsApp { get; set; }

        // Opciones para Correo
        public List<string> CorreosDestino { get; set; } = new List<string>();
        public string AsuntoCorreo { get; set; }
        public string MensajeCorreoHtml { get; set; }
    }
}
