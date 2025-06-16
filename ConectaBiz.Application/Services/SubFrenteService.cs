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
    public class SubFrenteService : ISubFrenteService
    {
        private readonly ISubFrenteRepository _subFrenteRepository;
        private readonly IFrenteRepository _frenteRepository;
        private readonly IMapper _mapper;

        public SubFrenteService(ISubFrenteRepository subFrenteRepository, IFrenteRepository frenteRepository, IMapper mapper)
        {
            _subFrenteRepository = subFrenteRepository;
            _frenteRepository = frenteRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SubFrenteDto>> GetAllAsync()
        {
            var subFrente = await _subFrenteRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<SubFrenteDto>>(subFrente);
        }

        public async Task<IEnumerable<SubFrenteDto>> GetActiveAsync()
        {
            var subFrente = await _subFrenteRepository.GetActiveAsync();
            return _mapper.Map<IEnumerable<SubFrenteDto>>(subFrente);
        }

        public async Task<IEnumerable<SubFrenteDto>> GetByFrenteIdAsync(int frenteId)
        {
            var subFrente = await _subFrenteRepository.GetByFrenteIdAsync(frenteId);
            return _mapper.Map<IEnumerable<SubFrenteDto>>(subFrente);
        }

        public async Task<SubFrenteDto?> GetByIdAsync(int id)
        {
            var subFrente = await _subFrenteRepository.GetByIdAsync(id);
            return subFrente == null ? null : _mapper.Map<SubFrenteDto>(subFrente);
        }

        public async Task<SubFrenteDto?> GetByIdWithFrenteAsync(int id)
        {
            var subFrente = await _subFrenteRepository.GetByIdWithFrenteAsync(id);
            return subFrente == null ? null : _mapper.Map<SubFrenteDto>(subFrente);
        }

        public async Task<SubFrenteDto> CreateAsync(SubFrenteDto subFrenteDto)
        {
            // Validar que el frente padre exista
            if (!await _frenteRepository.ExistsAsync(subFrenteDto.IdFrente))
                throw new KeyNotFoundException($"No se encontró el frente con ID {subFrenteDto.IdFrente}");

            // Validar que el código no exista
            if (await _subFrenteRepository.ExistsByCodigoAsync(subFrenteDto.Codigo))
                throw new InvalidOperationException($"Ya existe un sub-frente con el código '{subFrenteDto.Codigo}'");

            var subFrente = _mapper.Map<SubFrente>(subFrenteDto);
            var createdSubFrente = await _subFrenteRepository.CreateAsync(subFrente);
            return _mapper.Map<SubFrenteDto>(createdSubFrente);
        }

        public async Task<SubFrenteDto> UpdateAsync(int id, SubFrenteDto subFrenteDto)
        {
            var existingSubFrente = await _subFrenteRepository.GetByIdAsync(id);
            if (existingSubFrente == null)
                throw new KeyNotFoundException($"No se encontró el sub-frente con ID {id}");

            // Validar que el frente padre exista
            if (!await _frenteRepository.ExistsAsync(subFrenteDto.IdFrente))
                throw new KeyNotFoundException($"No se encontró el frente con ID {subFrenteDto.IdFrente}");

            // Validar que el código no exista en otro registro
            if (await _subFrenteRepository.ExistsByCodigoAsync(subFrenteDto.Codigo, id))
                throw new InvalidOperationException($"Ya existe un sub-frente con el código '{subFrenteDto.Codigo}'");

            _mapper.Map(subFrenteDto, existingSubFrente);
            existingSubFrente.Id = id; // Asegurar que el ID no cambie

            var updatedSubFrente = await _subFrenteRepository.UpdateAsync(existingSubFrente);
            return _mapper.Map<SubFrenteDto>(updatedSubFrente);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (!await _subFrenteRepository.ExistsAsync(id))
                return false;

            return await _subFrenteRepository.DeleteAsync(id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _subFrenteRepository.ExistsAsync(id);
        }
    }
}
