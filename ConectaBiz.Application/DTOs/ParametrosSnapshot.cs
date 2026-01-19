using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.DTOs
{
    public sealed class ParametrosSnapshot
    {
        public IReadOnlyList<Parametro> ListaParametros { get; init; } = Array.Empty<Parametro>();
        public IReadOnlyList<Parametro> ListaTipoTicket { get; init; } = Array.Empty<Parametro>();
        public IReadOnlyList<Parametro> ListaSubTipoTicket { get; init; } = Array.Empty<Parametro>();
        public IReadOnlyList<Parametro> ListaEstados { get; init; } = Array.Empty<Parametro>();
        public IReadOnlyList<Parametro> ListaPrioridades { get; init; } = Array.Empty<Parametro>();
        public IReadOnlyList<Parametro> ListaTipoActividad { get; init; } = Array.Empty<Parametro>();
    }
}
