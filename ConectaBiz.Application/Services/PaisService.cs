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
    public class PaisService : IPaisService
    {
        private readonly IPaisRepository _paisRepository;
        private readonly IMapper _mapper;

        public PaisService(IPaisRepository paisRepository, IMapper mapper)
        {
            _paisRepository = paisRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PaisDto>> GetAllAsync()
        {
            var paises = await _paisRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<PaisDto>>(paises);
        }

        public async Task<IEnumerable<PaisDto>> GetActiveAsync()
        {
            var paises = await _paisRepository.GetActiveAsync();
            return _mapper.Map<IEnumerable<PaisDto>>(paises);
        }

        public async Task<PaisDto?> GetByIdAsync(int id)
        {
            var pais = await _paisRepository.GetByIdAsync(id);
            return pais != null ? _mapper.Map<PaisDto>(pais) : null;
        }

        public async Task<PaisDto?> GetByCodigoAsync(string codigo)
        {
            var pais = await _paisRepository.GetByCodigoAsync(codigo);
            return pais != null ? _mapper.Map<PaisDto>(pais) : null;
        }

        public async Task<PaisDto> CreateAsync(CreatePaisDto createPaisDto)
        {
            // Validar que el código no exista
            if (await _paisRepository.ExistsByCodigoAsync(createPaisDto.Codigo))
            {
                throw new InvalidOperationException($"Ya existe un país con el código '{createPaisDto.Codigo}'");
            }

            var pais = _mapper.Map<Pais>(createPaisDto);
            var createdPais = await _paisRepository.CreateAsync(pais);
            return _mapper.Map<PaisDto>(createdPais);
        }

        public async Task<PaisDto> UpdateAsync(int id, UpdatePaisDto updatePaisDto)
        {
            var existingPais = await _paisRepository.GetByIdAsync(id);
            if (existingPais == null)
            {
                throw new KeyNotFoundException($"No se encontró el país con ID {id}");
            }

            // Validar que el código no exista en otro país
            if (await _paisRepository.ExistsByCodigoAsync(updatePaisDto.Codigo, id))
            {
                throw new InvalidOperationException($"Ya existe otro país con el código '{updatePaisDto.Codigo}'");
            }

            _mapper.Map(updatePaisDto, existingPais);
            var updatedPais = await _paisRepository.UpdateAsync(existingPais);
            return _mapper.Map<PaisDto>(updatedPais);
        }

        public async Task<bool> DeleteAsync(int id, string? usuarioModificacion = null)
        {
            if (!await _paisRepository.ExistsAsync(id))
            {
                throw new KeyNotFoundException($"No se encontró el país con ID {id}");
            }

            return await _paisRepository.DeleteAsync(id, usuarioModificacion);
        }
    }
}