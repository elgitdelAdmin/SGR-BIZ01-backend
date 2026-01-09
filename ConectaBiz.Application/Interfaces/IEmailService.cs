using ConectaBiz.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Interfaces
{
    public interface IEmailService
    {
        Task EnviarCorreosAsync(IEnumerable<string> destinatarios, string asunto, string mensajeTexto);
        Task EnviarCorreosConAdjuntosAsync(
           IEnumerable<string> destinatarios,
           string asunto,
           string mensajeTexto,
           IEnumerable<(string FileName, byte[] Content, string ContentType)> adjuntos
       );
    }
}
