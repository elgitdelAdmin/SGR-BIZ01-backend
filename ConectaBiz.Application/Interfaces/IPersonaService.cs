using ConectaBiz.Application.DTOs;
using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Interfaces
{
    public interface IPersonaService
    {
        Task<IEnumerable<PersonaDto>> GetAllAsync();
        Task<PersonaDto> GetByIdAsync(int id);
        Task<PersonaDto> CreateAsync(PersonaDto personaDto);
        Task<PersonaDto> UpdateAsync(int id, PersonaDto personaDto);
        Task<bool> DeleteAsync(int id);
    }
}
