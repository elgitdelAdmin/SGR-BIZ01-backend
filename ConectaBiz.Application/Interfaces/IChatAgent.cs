using ConectaBiz.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Interfaces
{
    public interface IChatAgent
    {
        Task<ChatResponseDto> PreguntarAsync(string mensaje);
    }
}