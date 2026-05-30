using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConectaBiz.Domain.Entities
{
    public class RawJsonConverter : JsonConverter<string>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            return jsonDoc.RootElement.GetRawText();
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            if (string.IsNullOrEmpty(value))
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteRawValue(value);
            }
        }
    }

    public class DashboardTicketConsultorDto
    {
        [JsonPropertyName("NombreCompleto")]
        public string? NombreCompleto { get; set; }

        [JsonPropertyName("NombreCompletoGestor")]
        public string? NombreCompletoGestor { get; set; }

        [JsonPropertyName("CodConecta")]
        public string? CodConecta { get; set; }

        [JsonPropertyName("CodMigracion")]
        public string? CodMigracion { get; set; }

        [JsonPropertyName("Titulo")]
        public string? Titulo { get; set; }

        [JsonPropertyName("EmpresaNombre")]
        public string? EmpresaNombre { get; set; }

        [JsonPropertyName("SocioNombre")]
        public string? SocioNombre { get; set; }

        [JsonPropertyName("EstadoTicket")]
        public string? EstadoTicket { get; set; }

        [JsonPropertyName("TipoTicket")]
        public string? TipoTicket { get; set; }

        [JsonPropertyName("SubtipoTicket")]
        public string? SubtipoTicket { get; set; }

        [JsonPropertyName("HorasPlanificadas")]
        public decimal HorasPlanificadas { get; set; }

        [JsonPropertyName("HorasRealizadas")]
        public decimal HorasRealizadas { get; set; }

        [JsonPropertyName("PorcentajeAvance")]
        public decimal? PorcentajeAvance { get; set; }

        [JsonPropertyName("FechaInicioPlanificada")]
        public DateTime? FechaInicioPlanificada { get; set; }

        [JsonPropertyName("FechaFinPlanificada")]
        public DateTime? FechaFinPlanificada { get; set; }

        [JsonPropertyName("FechaInicioReal")]
        public DateTime? FechaInicioReal { get; set; }

        [JsonPropertyName("FechaFinReal")]
        public DateTime? FechaFinReal { get; set; }

        [JsonPropertyName("DiasTranscurridosReal")]
        public int? DiasTranscurridosReal { get; set; }

        [JsonPropertyName("SemaforoFecha")]
        public string? SemaforoFecha { get; set; }

        [JsonPropertyName("DetallesPlanificacion")]
        [JsonConverter(typeof(RawJsonConverter))]
        public string? DetallesPlanificacion { get; set; }

        [JsonPropertyName("DetallesTareas")]
        [JsonConverter(typeof(RawJsonConverter))]
        public string? DetallesTareas { get; set; }
    }
}
