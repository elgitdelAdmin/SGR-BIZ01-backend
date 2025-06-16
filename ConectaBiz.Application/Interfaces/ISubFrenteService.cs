using ConectaBiz.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Interfaces
{
    public interface ISubFrenteService
    {
        Task<IEnumerable<SubFrenteDto>> GetAllAsync();
        Task<IEnumerable<SubFrenteDto>> GetActiveAsync();
        Task<IEnumerable<SubFrenteDto>> GetByFrenteIdAsync(int frenteId);
        Task<SubFrenteDto?> GetByIdAsync(int id);
        Task<SubFrenteDto?> GetByIdWithFrenteAsync(int id);
        Task<SubFrenteDto> CreateAsync(SubFrenteDto subFrenteDto);
        Task<SubFrenteDto> UpdateAsync(int id, SubFrenteDto subFrenteDto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
