using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ConectaBiz.Domain.Services
{
    public static class SubFrenteMatchingService
    {
        public static SubFrente? BuscarSubFrentePorGrupo(string? grupo, IEnumerable<SubFrente> subFrentes)
        {
            if (string.IsNullOrWhiteSpace(grupo) || subFrentes == null) return null;
            var grupoTrimmed = grupo.Trim();
            
            return subFrentes.FirstOrDefault(sf =>
                ParseValor1(sf.Valor1).Any(v => v.Equals(grupoTrimmed, StringComparison.OrdinalIgnoreCase)));
        }

        private static List<string> ParseValor1(string? valor1)
        {
            if (string.IsNullOrWhiteSpace(valor1)) return new List<string>();
            var trimmed = valor1.Trim();
            if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
            {
                try { return JsonSerializer.Deserialize<List<string>>(trimmed) ?? new List<string>(); }
                catch { }
            }
            return new List<string> { trimmed };
        }
    }
}
