using System.Collections.Generic;

namespace ConectaBiz.Application.DTOs
{
    public class WhatsAppJobSettings
    {
        public int IntervaloMinutos { get; set; } = 60;
        public string TelefonoAdicional { get; set; }
        public List<string> HorasEnvioGestoresCuenta { get; set; } = new List<string>();
        public List<string> HorasEnvioGestoresConsultoria { get; set; } = new List<string>();
    }
}
