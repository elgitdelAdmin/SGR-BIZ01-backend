using System;
using System.Globalization;

namespace ConectaBiz.Domain.Services
{
    public static class CargaMasivaDataFormatterService
    {
        public static DateTime ParsearFechaEstandar(string fechaTexto, string codTicket)
        {
            if (string.IsNullOrWhiteSpace(fechaTexto))
                throw new Exception($"Fecha vacía o nula para ticket {codTicket}");

            bool ok = DateTime.TryParseExact(
                fechaTexto.Trim(),
                new[]
                {
                    "yyyy-MM-dd HH:mm:ss", "yyyy-M-d HH:mm:ss",
                    "d/M/yyyy HH:mm:ss", "d/M/yyyy  HH:mm:ss",
                    "M/d/yy HH:mm", "MM/dd/yy HH:mm",
                    "dd-MMM-yyyy", "dd/MM/yyyy HH:mm:ss",
                    "M/d/yy", "d/M/yy",
                    "MM/dd/yyyy", "dd/MM/yyyy",
                    "M/d/yyyy", "d/M/yyyy"
                },
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out var fecha);

            if (!ok)
                throw new Exception($"No se pudo parsear la fecha '{fechaTexto}' para ticket {codTicket}.");

            return DateTime.SpecifyKind(fecha, DateTimeKind.Local);
        }

        public static string LimpiarCodTicketInternoEstandar(string codTicket)
        {
            if (string.IsNullOrWhiteSpace(codTicket)) return string.Empty;
            return codTicket
                .Replace("Solicitud", "", StringComparison.OrdinalIgnoreCase)
                .Replace("Incidente", "", StringComparison.OrdinalIgnoreCase)
                .Replace(" ", "")
                .Trim();
        }
    }
}
