
namespace ConectaBiz.Application.DTOs
{
    public class ChatResponseDto
    {
        public object Respuesta { get; set; } = new { };
        public string ModeloUsado { get; set; } = string.Empty;
        public Dictionary<string, object> MetadatosAdicionales { get; set; } = new Dictionary<string, object>();
        public List<object>? ErroresFallback { get; set; }
    }
}