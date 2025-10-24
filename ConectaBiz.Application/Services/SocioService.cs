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
    public class SocioService : ISocioService
    {
        private readonly ISocioRepository _socioRepository;
        private readonly IMapper _mapper;

        public SocioService(ISocioRepository socioRepository, IMapper mapper)
        {
            _socioRepository = socioRepository;
            _mapper = mapper;
        }

        public async Task<List<SocioDto>> ListarTodosAsync()
        {
            var socios = await _socioRepository.ListarTodosAsync();
            return _mapper.Map<List<SocioDto>>(socios);
        }
        public async Task<SocioDto?> ObtenerPorIdAsync(int id)
        {
                var socio = await _socioRepository.ObtenerPorIdAsync(id);
                return socio != null ? _mapper.Map<SocioDto>(socio) : null;
        }

        public async Task<SocioDto?> ObtenerPorNumDocAsync(string numDoc)
        {
                var socio = await _socioRepository.ObtenerPorNumDocAsync(numDoc);
                return socio != null ? _mapper.Map<SocioDto>(socio) : null;
        }

        public async Task<SocioDto> CrearAsync(SocioCreateDto socioCreateDto)
        {
                if (!string.IsNullOrWhiteSpace(socioCreateDto.NumDocContribuyente))
                {
                    var existe = await _socioRepository.ExisteNumDocAsync(socioCreateDto.NumDocContribuyente);
                    if (existe)
                    {
                        throw new InvalidOperationException($"Ya existe un socio con el número de documento: {socioCreateDto.NumDocContribuyente}");
                    }
                }

                var socio = _mapper.Map<Socio>(socioCreateDto);
                socio.FechaRegistro = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                socio.Activo = true;

                var socioCreado = await _socioRepository.CrearAsync(socio);
                return _mapper.Map<SocioDto>(socioCreado);
        }

        public async Task<SocioDto> ActualizarAsync(int id, SocioUpdateDto socioUpdateDto)
        {
                var socioExistente = await _socioRepository.ObtenerPorIdAsync(id);
                if (socioExistente == null)
                {
                    throw new KeyNotFoundException($"No se encontró el socio con ID: {id}");
                }

                if (!socioExistente.Activo)
                {
                    throw new InvalidOperationException($"No se puede actualizar un socio inactivo");
                }

                // Mapear solo los campos que se pueden actualizar
                socioExistente.RazonSocial = socioUpdateDto.RazonSocial;
                socioExistente.Codigo = socioUpdateDto.Codigo;
                socioExistente.Nombre = socioUpdateDto.Nombre;
                socioExistente.NombreComercial = socioUpdateDto.NombreComercial;
                socioExistente.Direccion = socioUpdateDto.Direccion;
                socioExistente.Telefono1 = socioUpdateDto.Telefono1;
                socioExistente.Telefono2 = socioUpdateDto.Telefono2;
                socioExistente.Email = socioUpdateDto.Email;
                socioExistente.FechaModificacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            socioExistente.UsuarioModificacion = socioUpdateDto.UsuarioModificacion;

                var socioActualizado = await _socioRepository.ActualizarAsync(socioExistente);
                return _mapper.Map<SocioDto>(socioActualizado);
        }

        public async Task<bool> EliminarAsync(int id)
        {
                var socio = await _socioRepository.ObtenerPorIdAsync(id);
                if (socio == null)
                {
                    throw new KeyNotFoundException($"No se encontró el socio con ID: {id}");
                }

                return await _socioRepository.EliminarAsync(id);
        }
    }
}
