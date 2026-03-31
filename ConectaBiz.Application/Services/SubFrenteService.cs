using AutoMapper;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Exceptions;
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
        private readonly IConsultorFrenteSubFrenteRepository _consultorFrenteSubFrenteRepository;
        private readonly IMapper _mapper;

        public SubFrenteService(ISubFrenteRepository subFrenteRepository, IFrenteRepository frenteRepository, IConsultorFrenteSubFrenteRepository consultorFrenteSubFrenteRepository, IMapper mapper)
        {
            _subFrenteRepository = subFrenteRepository;
            _frenteRepository = frenteRepository;
            _consultorFrenteSubFrenteRepository = consultorFrenteSubFrenteRepository;
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

        public async Task<SubFrenteDto> CreateAsync(CreateSubFrenteDto createSubFrenteDto)
        {
            // Validar que el frente padre exista
            if (!await _frenteRepository.ExistsAsync(createSubFrenteDto.IdFrente))
                throw new KeyNotFoundException($"No se encontró el frente con ID {createSubFrenteDto.IdFrente}");

            var subFrente = _mapper.Map<SubFrente>(createSubFrenteDto);
            
            // Asignar código temporal por restricción NOT NULL
            subFrente.Codigo = "TMP-" + Guid.NewGuid().ToString().Substring(0,8);

            // Se genera el ID al crear en base de datos
            var createdSubFrente = await _subFrenteRepository.CreateAsync(subFrente);

            // Asigna el ID final como pidió el usuario (Ej: SFR023)
            createdSubFrente.Codigo = $"SFR{createdSubFrente.Id:D3}";

            // Actualiza la entidad con su código final
            createdSubFrente = await _subFrenteRepository.UpdateAsync(createdSubFrente);

            return _mapper.Map<SubFrenteDto>(createdSubFrente);
        }

        public async Task<SubFrenteDto> UpdateAsync(int id, UpdateSubFrenteDto updateSubFrenteDto)
        {
            var existingSubFrente = await _subFrenteRepository.GetByIdAsync(id);
            if (existingSubFrente == null)
                throw new KeyNotFoundException($"No se encontró el sub-frente con ID {id}");

            // Validar que el frente padre exista
            if (!await _frenteRepository.ExistsAsync(updateSubFrenteDto.IdFrente))
                throw new KeyNotFoundException($"No se encontró el frente con ID {updateSubFrenteDto.IdFrente}");

            // Validar que el código no exista en otro registro
            if (await _subFrenteRepository.ExistsByCodigoAsync(updateSubFrenteDto.Codigo, id))
                throw new InvalidOperationException($"Ya existe un sub-frente con el código '{updateSubFrenteDto.Codigo}'");

            // Validar consultores asociados al intentar desactivar
            if (existingSubFrente.Activo && !updateSubFrenteDto.Activo)
            {
                var consultores = (await GetConsultoresAsociadosBySubFrenteIdAsync(id)).ToList();
                if (consultores.Any())
                    throw new ConsultoresAsociadosException(
                        "No se puede desactivar el sub-frente porque tiene consultores asociados.",
                        consultores);
            }

            _mapper.Map(updateSubFrenteDto, existingSubFrente);
            existingSubFrente.Id = id; // Asegurar que el ID no cambie

            var updatedSubFrente = await _subFrenteRepository.UpdateAsync(existingSubFrente);
            return _mapper.Map<SubFrenteDto>(updatedSubFrente);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (!await _subFrenteRepository.ExistsAsync(id))
                return false;

            // Validar consultores asociados antes de desactivar
            var consultores = (await GetConsultoresAsociadosBySubFrenteIdAsync(id)).ToList();
            if (consultores.Any())
                throw new ConsultoresAsociadosException(
                    "No se puede desactivar el sub-frente porque tiene consultores asociados.",
                    consultores);

            return await _subFrenteRepository.DeleteAsync(id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _subFrenteRepository.ExistsAsync(id);
        }

        public async Task<IEnumerable<ConsultorAsociadoDto>> GetConsultoresAsociadosBySubFrenteIdAsync(int subFrenteId)
        {
            var registros = await _consultorFrenteSubFrenteRepository.GetActiveBySubFrenteIdAsync(subFrenteId);
            return registros.Select(r => new ConsultorAsociadoDto
            {
                ConsultorId = r.ConsultorId,
                NombreCompleto = r.Consultor?.Persona != null
                    ? $"{r.Consultor.Persona.Nombres} {r.Consultor.Persona.ApellidoPaterno} {r.Consultor.Persona.ApellidoMaterno}".Trim()
                    : "Sin nombre",
                FrenteNombre = r.Frente?.Nombre,
                SubFrenteNombre = r.SubFrente?.Nombre
            }).ToList();
        }
    }
}
