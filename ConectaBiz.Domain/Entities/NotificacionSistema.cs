using System;

namespace ConectaBiz.Domain.Entities
{
    public class NotificacionSistema
    {
        public int Id { get; set; }
        public int? IdUser { get; set; }
        public string TipoNotificacion { get; set; }
        public int? IdReferencia { get; set; }
        public string RutaFrontend { get; set; }
        public string Mensaje { get; set; }
        public bool Leido { get; set; } = false;
        public DateTime? FechaLectura { get; set; }
        public string CanalesEnviados { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public bool Activo { get; set; } = true;
    }
}
