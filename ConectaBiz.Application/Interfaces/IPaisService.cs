using ConectaBiz.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Interfaces
{
    public interface IPaisService
    {
        Task<IEnumerable<PaisDto>> GetAllAsync();
        Task<IEnumerable<PaisDto>> GetActiveAsync();
        Task<PaisDto?> GetByIdAsync(int id);
        Task<PaisDto?> GetByCodigoAsync(string codigo);
        Task<PaisDto> CreateAsync(CreatePaisDto createPaisDto);
        Task<PaisDto> UpdateAsync(int id, UpdatePaisDto updatePaisDto);
        Task<bool> DeleteAsync(int id, string? usuarioModificacion = null);
    }
}
