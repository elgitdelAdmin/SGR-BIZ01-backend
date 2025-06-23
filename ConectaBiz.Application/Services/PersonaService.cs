using AutoMapper;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Services
{
    public class PersonaService : IPersonaService
    {
        private readonly IPersonaRepository _personaRepository;
        private readonly IMapper _mapper;

        public PersonaService(IPersonaRepository personaRepository, IMapper mapper)
        {
            _personaRepository = personaRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PersonaDto>> GetAllAsync()
        {
            var personas = await _personaRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<PersonaDto>>(personas);
        }

        public async Task<PersonaDto> GetByIdAsync(int id)
        {
            var persona = await _personaRepository.GetByIdAsync(id);
            return persona != null ? _mapper.Map<PersonaDto>(persona) : null;
        }

        public async Task<PersonaDto> CreateAsync(PersonaDto personaDto)
        {
            var persona = _mapper.Map<Persona>(personaDto);
            var result = await _personaRepository.CreateAsync(persona);
            return _mapper.Map<PersonaDto>(result);
        }

        public async Task<PersonaDto> UpdateAsync(int id, PersonaDto personaDto)
        {
            // Verificar si la persona existe
            if (!await _personaRepository.ExistsAsync(id))
                return null;

            personaDto.Id = id; // Asegurar que el ID sea el correcto
            var persona = _mapper.Map<Persona>(personaDto);
            var result = await _personaRepository.UpdateAsync(persona);
            return result != null ? _mapper.Map<PersonaDto>(result) : null;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _personaRepository.DeleteAsync(id);
        }
    }
}
