using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ConectaBiz.Domain.Services
{
    public static class ConsultorMatchingService
    {
        private static readonly List<string> PalabrasNoDeseadas = new()
        {
            "csti_facturación", "csti_finanzas", "csti_compras",
            "csti_rrhh", "csti_abap", "csti_pp", "csti_controlling", "csti_"
        };

        public static T? BuscarMejorCoincidencia<T>(
            string nombreCompleto,
            IEnumerable<T> consultores,
            Func<T, string> obtenerNombreCompleto) where T : class
        {
            if (string.IsNullOrWhiteSpace(nombreCompleto) || consultores == null)
                return null;

            var partes = nombreCompleto
                .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim().ToLower())
                .Where(p => !PalabrasNoDeseadas.Any(nd => p.Contains(nd)))
                .ToList();

            if (partes.Count == 0) return null;

            foreach (var consultor in consultores)
            {
                var nombre = obtenerNombreCompleto(consultor).ToLower();
                if (partes.All(p => nombre.Contains(p)))
                    return consultor;
            }

            var partesPorComa = nombreCompleto
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .SelectMany(f => f.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Select(p => p.Trim().ToLower())
                .ToList();

            T? mejorCoincidencia = null;
            int maxCoincidencias = 0;

            foreach (var consultor in consultores)
            {
                var nombre = obtenerNombreCompleto(consultor).ToLower();
                int coincidencias = partesPorComa.Count(p => nombre.Contains(p));
                if (coincidencias > maxCoincidencias)
                {
                    maxCoincidencias = coincidencias;
                    mejorCoincidencia = consultor;
                }
            }
            if (maxCoincidencias > 0) return mejorCoincidencia;

            var partesSinTilde = partesPorComa.Select(QuitarTildes).ToList();
            foreach (var consultor in consultores)
            {
                var nombre = QuitarTildes(obtenerNombreCompleto(consultor).ToLower());
                if (partesSinTilde.All(p => nombre.Contains(p)))
                    return consultor;
            }

            var partesSinExt = nombreCompleto
                .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(p =>
                {
                    var palabra = p.Trim();
                    return !palabra.Equals("_ext", StringComparison.OrdinalIgnoreCase)
                        && !palabra.Contains("_ext", StringComparison.OrdinalIgnoreCase)
                        && !palabra.Contains("ext_", StringComparison.OrdinalIgnoreCase);
                })
                .Select(p => p.Trim().ToLower())
                .ToList();

            if (partesSinExt.Count > 0)
            {
                foreach (var consultor in consultores)
                {
                    var nombre = obtenerNombreCompleto(consultor).ToLower();
                    if (partesSinExt.All(p => nombre.Contains(p)))
                        return consultor;
                }
            }

            return null;
        }

        private static string QuitarTildes(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return texto;
            var normalized = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
