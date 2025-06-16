using ConectaBiz.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Interfaces
{
    public interface IFrenteService
    {
        Task<IEnumerable<FrenteDto>> GetAllAsync();
        Task<IEnumerable<FrenteDto>> GetActiveAsync();
        Task<FrenteDto?> GetByIdAsync(int id);
        Task<FrenteDto?> GetByIdWithSubFrentesAsync(int id);
        Task<FrenteDto> CreateAsync(FrenteDto frenteDto);
        Task<FrenteDto> UpdateAsync(int id, FrenteDto frenteDto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
