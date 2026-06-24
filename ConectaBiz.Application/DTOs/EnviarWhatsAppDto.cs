using System.Collections.Generic;

namespace ConectaBiz.Application.DTOs
{
    public class EnviarWhatsAppDto
    {
        public string? Remitente { get; set; }
        public List<string> Telefonos { get; set; } = new();
        public string Mensaje { get; set; } = string.Empty;
    }
}
