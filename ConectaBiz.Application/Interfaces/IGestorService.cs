using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Interfaces
{
    public interface IGestorService
    {
        Task<IEnumerable<GestorDto>> GetAllAsync();
        Task<IEnumerable<GestorDto>> GetByIdSocio(int idSocio);
        Task<GestorDto?> GetByIdAsync(int id);
        Task<GestorDto?> GetByIdUserAsync(int iduser);
        Task<GestorDto> CreateAsync(CreateGestorDto createGestorDto);
        Task<GestorDto> UpdateAsync(int id, UpdateGestorDto updateGestorDto);
        Task<bool> DeleteAsync(int id);
    }
}
