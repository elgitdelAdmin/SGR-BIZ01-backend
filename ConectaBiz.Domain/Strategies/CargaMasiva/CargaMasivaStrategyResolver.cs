using System;
using System.Collections.Generic;
using System.Linq;

namespace ConectaBiz.Domain.Strategies.CargaMasiva
{
    public class CargaMasivaStrategyResolver
    {
        private readonly IEnumerable<ICargaMasivaEmpresaStrategy> _strategies;

        public CargaMasivaStrategyResolver(IEnumerable<ICargaMasivaEmpresaStrategy> strategies)
        {
            _strategies = strategies;
        }

        public ICargaMasivaEmpresaStrategy Resolver(string tipoCarga)
        {
            var strategy = _strategies.FirstOrDefault(s => s.PuedeResolver(tipoCarga));
            if (strategy == null)
                throw new InvalidOperationException($"No existe estrategia de carga masiva para tipo '{tipoCarga}'. Tipos soportados: {string.Join(", ", _strategies.Select(s => s.GetType().Name))}");
            return strategy;
        }
    }
}
