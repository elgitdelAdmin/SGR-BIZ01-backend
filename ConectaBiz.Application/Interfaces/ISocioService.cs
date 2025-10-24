using ConectaBiz.Application.DTOs;
using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Interfaces
{
    public interface ISocioService
    {
        Task<List<SocioDto>> ListarTodosAsync();
        Task<SocioDto?> ObtenerPorIdAsync(int id);
        Task<SocioDto?> ObtenerPorNumDocAsync(string numDoc);
        Task<SocioDto> CrearAsync(SocioCreateDto socioCreateDto);
        Task<SocioDto> ActualizarAsync(int id, SocioUpdateDto socioUpdateDto);
        Task<bool> EliminarAsync(int id);
    }
}
