using ConectaBiz.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Interfaces
{
    public interface IParametroService
    {
        Task<IEnumerable<ParametroDto>> GetAllAsync();
        Task<ParametroDto?> GetByIdAsync(int id);
        Task<IEnumerable<ParametroDto>> GetByTipoParametroAsync(string tipoParametro);
        Task<IEnumerable<ParametroDto>> GetActivosAsync();
        Task<ParametroDto?> GetByCodigoAsync(string tipoParametro, string codigo);
        Task<ParametroDto> CreateAsync(CreateParametroDto createDto);
        Task<ParametroDto> UpdateAsync(int id, UpdateParametroDto updateDto);
        Task<bool> DeleteAsync(int id);
    }
}