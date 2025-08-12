using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Services;
using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Interfaces
{
    public interface IConsultorService
    {
        Task<IEnumerable<ConsultorDto>> GetAllAsync();
        Task<ConsultorDto> GetByIdAsync(int id);
        Task<ConsultorDto> GetByIdPersonaAsync(int idPersona);
        Task<IEnumerable<ConsultorDto>> GetByIdSocioAsync(int idSocio);
        Task<ConsultorDto> GetByIdUserAsync(int iduser);
        Task<ConsultorDto> CreateAsync(ConsultorDto consultorDto);
        Task<ConsultorDto> UpdateAsync(int id, ConsultorDto consultorDto);
        Task<bool> DeleteAsync(int id);
    }
}
