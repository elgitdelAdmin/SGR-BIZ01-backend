using ConectaBiz.Application.DTOs;
using System;
using System.Collections.Generic;

namespace ConectaBiz.Application.Exceptions
{
    /// <summary>
    /// Excepción que se lanza cuando se intenta desactivar un Frente o SubFrente
    /// que tiene consultores activos asociados.
    /// </summary>
    public class ConsultoresAsociadosException : InvalidOperationException
    {
        public IEnumerable<ConsultorAsociadoDto> Consultores { get; }

        public ConsultoresAsociadosException(string message, IEnumerable<ConsultorAsociadoDto> consultores)
            : base(message)
        {
            Consultores = consultores;
        }
    }
}
