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
    public class FrenteService : IFrenteService
    {
        private readonly IFrenteRepository _frenteRepository;
        private readonly IMapper _mapper;

        public FrenteService(IFrenteRepository frenteRepository, IMapper mapper)
        {
            _frenteRepository = frenteRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FrenteDto>> GetAllAsync()
        {
            var frentes = await _frenteRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<FrenteDto>>(frentes);
        }

        public async Task<IEnumerable<FrenteDto>> GetActiveAsync()
        {
            var frentes = await _frenteRepository.GetActiveAsync();
            return _mapper.Map<IEnumerable<FrenteDto>>(frentes);
        }

        public async Task<FrenteDto?> GetByIdAsync(int id)
        {
            var frente = await _frenteRepository.GetByIdAsync(id);
            return frente == null ? null : _mapper.Map<FrenteDto>(frente);
        }

        public async Task<FrenteDto?> GetByIdWithSubFrentesAsync(int id)
        {
            var frente = await _frenteRepository.GetByIdWithSubFrentesAsync(id);
            return frente == null ? null : _mapper.Map<FrenteDto>(frente);
        }

        public async Task<FrenteDto> CreateAsync(FrenteDto frenteDto)
        {
            // Validar que el código no exista
            if (await _frenteRepository.ExistsByCodigoAsync(frenteDto.Codigo))
                throw new InvalidOperationException($"Ya existe un frente con el código '{frenteDto.Codigo}'");

            var frente = _mapper.Map<Frente>(frenteDto);
            var createdFrente = await _frenteRepository.CreateAsync(frente);
            return _mapper.Map<FrenteDto>(createdFrente);
        }

        public async Task<FrenteDto> UpdateAsync(int id, FrenteDto frenteDto)
        {
            var existingFrente = await _frenteRepository.GetByIdAsync(id);
            if (existingFrente == null)
                throw new KeyNotFoundException($"No se encontró el frente con ID {id}");

            // Validar que el código no exista en otro registro
            if (await _frenteRepository.ExistsByCodigoAsync(frenteDto.Codigo, id))
                throw new InvalidOperationException($"Ya existe un frente con el código '{frenteDto.Codigo}'");

            _mapper.Map(frenteDto, existingFrente);
            existingFrente.Id = id; // Asegurar que el ID no cambie

            var updatedFrente = await _frenteRepository.UpdateAsync(existingFrente);
            return _mapper.Map<FrenteDto>(updatedFrente);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (!await _frenteRepository.ExistsAsync(id))
                return false;

            return await _frenteRepository.DeleteAsync(id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _frenteRepository.ExistsAsync(id);
        }
    }
}
