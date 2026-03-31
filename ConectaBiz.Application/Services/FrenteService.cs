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
    public class FrenteService : IFrenteService
    {
        private readonly IFrenteRepository _frenteRepository;
        private readonly IConsultorFrenteSubFrenteRepository _consultorFrenteSubFrenteRepository;
        private readonly IMapper _mapper;

        public FrenteService(IFrenteRepository frenteRepository, IConsultorFrenteSubFrenteRepository consultorFrenteSubFrenteRepository, IMapper mapper)
        {
            _frenteRepository = frenteRepository;
            _consultorFrenteSubFrenteRepository = consultorFrenteSubFrenteRepository;
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

        public async Task<FrenteDto> CreateAsync(CreateFrenteDto createFrenteDto)
        {
            var frente = _mapper.Map<Frente>(createFrenteDto);
            
            // Asignar un c\u00f3digo temporal para superar posible restricci\u00f3n NOT NULL
            frente.Codigo = "TMP-" + Guid.NewGuid().ToString().Substring(0,8);

            // Primer guardado: genera el ID
            var createdFrente = await _frenteRepository.CreateAsync(frente);

            // Asignar el c\u00f3digo real basado en el ID ya generado (Ej: FRN004)
            createdFrente.Codigo = $"FRN{createdFrente.Id:D3}";

            // Segundo guardado: actualiza el c\u00f3digo final en base de datos
            createdFrente = await _frenteRepository.UpdateAsync(createdFrente);

            return _mapper.Map<FrenteDto>(createdFrente);
        }

        public async Task<FrenteDto> UpdateAsync(int id, UpdateFrenteDto updateFrenteDto)
        {
            var existingFrente = await _frenteRepository.GetByIdAsync(id);
            if (existingFrente == null)
                throw new KeyNotFoundException($"No se encontró el frente con ID {id}");

            // Validar que el código no exista en otro registro
            if (await _frenteRepository.ExistsByCodigoAsync(updateFrenteDto.Codigo, id))
                throw new InvalidOperationException($"Ya existe un frente con el código '{updateFrenteDto.Codigo}'");

            // Validar consultores asociados al intentar desactivar
            if (existingFrente.Activo && !updateFrenteDto.Activo)
            {
                var consultores = (await GetConsultoresAsociadosByFrenteIdAsync(id)).ToList();
                if (consultores.Any())
                    throw new ConsultoresAsociadosException(
                        "No se puede desactivar el frente porque tiene consultores asociados.",
                        consultores);
            }

            _mapper.Map(updateFrenteDto, existingFrente);
            existingFrente.Id = id; // Asegurar que el ID no cambie

            var updatedFrente = await _frenteRepository.UpdateAsync(existingFrente);
            return _mapper.Map<FrenteDto>(updatedFrente);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (!await _frenteRepository.ExistsAsync(id))
                return false;

            // Validar consultores asociados antes de desactivar
            var consultores = (await GetConsultoresAsociadosByFrenteIdAsync(id)).ToList();
            if (consultores.Any())
                throw new ConsultoresAsociadosException(
                    "No se puede desactivar el frente porque tiene consultores asociados.",
                    consultores);

            return await _frenteRepository.DeleteAsync(id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _frenteRepository.ExistsAsync(id);
        }

        public async Task<IEnumerable<ConsultorAsociadoDto>> GetConsultoresAsociadosByFrenteIdAsync(int frenteId)
        {
            var registros = await _consultorFrenteSubFrenteRepository.GetActiveByFrenteIdAsync(frenteId);
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
