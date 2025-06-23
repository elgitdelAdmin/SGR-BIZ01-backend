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
    public class ParametroService : IParametroService
    {
        private readonly IParametroRepository _repository;
        private readonly IMapper _mapper;

        public ParametroService(IParametroRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ParametroDto>> GetAllAsync()
        {
            var parametros = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<ParametroDto>>(parametros);
        }

        public async Task<ParametroDto?> GetByIdAsync(int id)
        {
            var parametro = await _repository.GetByIdAsync(id);
            return parametro != null ? _mapper.Map<ParametroDto>(parametro) : null;
        }

        public async Task<IEnumerable<ParametroDto>> GetByTipoParametroAsync(string tipoParametro)
        {
            var parametros = await _repository.GetByTipoParametroAsync(tipoParametro);
            return _mapper.Map<IEnumerable<ParametroDto>>(parametros);
        }

        public async Task<IEnumerable<ParametroDto>> GetActivosAsync()
        {
            var parametros = await _repository.GetActivosAsync();
            return _mapper.Map<IEnumerable<ParametroDto>>(parametros);
        }

        public async Task<ParametroDto?> GetByCodigoAsync(string tipoParametro, string codigo)
        {
            var parametro = await _repository.GetByCodigoAsync(tipoParametro, codigo);
            return parametro != null ? _mapper.Map<ParametroDto>(parametro) : null;
        }

        public async Task<ParametroDto> CreateAsync(CreateParametroDto createDto)
        {
            // Validar que no existe el mismo código para el tipo de parámetro
            if (await _repository.ExistsAsync(createDto.TipoParametro, createDto.Codigo))
            {
                throw new InvalidOperationException($"Ya existe un parámetro con el código '{createDto.Codigo}' para el tipo '{createDto.TipoParametro}'");
            }

            var parametro = _mapper.Map<Parametro>(createDto);
            var createdParametro = await _repository.CreateAsync(parametro);
            return _mapper.Map<ParametroDto>(createdParametro);
        }

        public async Task<ParametroDto> UpdateAsync(int id, UpdateParametroDto updateDto)
        {
            var existingParametro = await _repository.GetByIdAsync(id);
            if (existingParametro == null)
            {
                throw new KeyNotFoundException($"No se encontró el parámetro con ID {id}");
            }

            // Validar que no existe otro parámetro con el mismo código para el tipo
            if (await _repository.ExistsAsync(updateDto.TipoParametro, updateDto.Codigo, id))
            {
                throw new InvalidOperationException($"Ya existe otro parámetro con el código '{updateDto.Codigo}' para el tipo '{updateDto.TipoParametro}'");
            }

            _mapper.Map(updateDto, existingParametro);
            var updatedParametro = await _repository.UpdateAsync(existingParametro);
            return _mapper.Map<ParametroDto>(updatedParametro);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}
