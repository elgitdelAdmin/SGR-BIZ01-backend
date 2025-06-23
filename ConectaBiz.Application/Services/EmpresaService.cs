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
    public class EmpresaService : IEmpresaService
    {
        private readonly IEmpresaRepository _empresaRepository;
        private readonly IPersonaRepository _personaRepository;
        private readonly IMapper _mapper;

        public EmpresaService(
            IEmpresaRepository empresaRepository,
            IPersonaRepository personaRepository,
            IMapper mapper)
        {
            _empresaRepository = empresaRepository;
            _personaRepository = personaRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<EmpresaDto>> GetAllAsync()
        {
            var empresas = await _empresaRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<EmpresaDto>>(empresas);
        }

        public async Task<IEnumerable<EmpresaDto>> GetAllActiveAsync()
        {
            var empresas = await _empresaRepository.GetAllActiveAsync();
            return _mapper.Map<IEnumerable<EmpresaDto>>(empresas);
        }

        public async Task<EmpresaDto?> GetByIdAsync(int id)
        {
            var empresa = await _empresaRepository.GetByIdAsync(id);
            return empresa != null ? _mapper.Map<EmpresaDto>(empresa) : null;
        }

        public async Task<EmpresaDto?> GetByCodigoAsync(string codigo)
        {
            var empresa = await _empresaRepository.GetByCodigoAsync(codigo);
            return empresa != null ? _mapper.Map<EmpresaDto>(empresa) : null;
        }

        public async Task<IEnumerable<EmpresaDto>> GetBySocioAsync(int idSocio)
        {
            var empresas = await _empresaRepository.GetBySocioAsync(idSocio);
            return _mapper.Map<IEnumerable<EmpresaDto>>(empresas);
        }

        public async Task<IEnumerable<EmpresaDto>> GetByGestorAsync(int idGestor)
        {
            var empresas = await _empresaRepository.GetByGestorAsync(idGestor);
            return _mapper.Map<IEnumerable<EmpresaDto>>(empresas);
        }

        public async Task<EmpresaDto> CreateAsync(CreateEmpresaDto createDto)
        {
            // Validar que el código no exista
            if (await _empresaRepository.ExistsByNumDocYPaisAsync(createDto.NumDocContribuyente, createDto.IdPais))
            {
                throw new InvalidOperationException($"Ya existe una empresa con el Número de Documento '{createDto.NumDocContribuyente}' que pertenece al pais seleccionado.");
            }

            // Variable para almacenar el ID de la persona responsable
            int personaId = 0;

            // Validar y procesar información de persona
            if (createDto.Persona != null)
            {
                if (!string.IsNullOrEmpty(createDto.Persona.NumeroDocumento))
                {
                    // Buscar persona existente por tipo de documento y número
                    var personaExistente = await _personaRepository.GetByTipoNumDocumentoAsync(
                        createDto.Persona.TipoDocumento,
                        createDto.Persona.NumeroDocumento);

                    if (personaExistente != null)
                    {
                        // Si la persona existe, actualizarla con los nuevos datos
                        personaExistente.Nombres = createDto.Persona.Nombres;
                        personaExistente.ApellidoMaterno = createDto.Persona.ApellidoMaterno;
                        personaExistente.ApellidoPaterno = createDto.Persona.ApellidoPaterno;
                        personaExistente.Telefono = createDto.Persona.Telefono;
                        personaExistente.Telefono2 = createDto.Persona.Telefono2;
                        personaExistente.Correo = createDto.Persona.Correo;
                        personaExistente.Direccion = createDto.Persona.Direccion;
                        personaExistente.FechaNacimiento = createDto.Persona.FechaNacimiento.HasValue
                            ? DateTime.SpecifyKind(createDto.Persona.FechaNacimiento.Value, DateTimeKind.Local)
                            : null;
                        personaExistente.FechaActualizacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

                        var personaActualizada = await _personaRepository.UpdateAsync(personaExistente);
                        personaId = personaActualizada.Id;
                    }
                    else
                    {
                        // Si la persona no existe, crearla
                        var nuevaPersona = _mapper.Map<Persona>(createDto.Persona);
                        nuevaPersona.FechaNacimiento = nuevaPersona.FechaNacimiento.HasValue
                            ? DateTime.SpecifyKind(nuevaPersona.FechaNacimiento.Value, DateTimeKind.Local)
                            : null;
                        nuevaPersona.FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                        nuevaPersona.FechaActualizacion = null;
                        nuevaPersona.Activo = true;

                        var personaCreada = await _personaRepository.CreateAsync(nuevaPersona);
                        personaId = personaCreada.Id;
                    }
                }
                else
                {
                    throw new InvalidOperationException("Se debe proporcionar un número de documento válido para la persona responsable");
                }
            }
            else
            {
                throw new InvalidOperationException("Se debe proporcionar información de la persona responsable para crear una empresa");
            }

            // Mapear el DTO a la entidad Empresa
            var empresa = _mapper.Map<Empresa>(createDto);
            empresa.FechaRegistro = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

            // Asignar el ID de la persona responsable (asumiendo que tu entidad Empresa tiene esta propiedad)
            empresa.IdPersonaResponsable = personaId;

            // Crear la empresa
            var createdEmpresa = await _empresaRepository.CreateAsync(empresa);

            // Obtener la empresa creada con las relaciones
            var empresaWithRelations = await _empresaRepository.GetByIdAsync(createdEmpresa.Id);
            return _mapper.Map<EmpresaDto>(empresaWithRelations);
        }

        public async Task<EmpresaDto> UpdateAsync(int id, UpdateEmpresaDto updateDto)
        {
            var existingEmpresa = await _empresaRepository.GetByIdAsync(id);
            if (existingEmpresa == null)
            {
                throw new KeyNotFoundException($"No se encontró la empresa con ID {id}");
            }


            // Mantener valores originales que no deben cambiar
            var fechaRegistroOriginal = existingEmpresa.FechaRegistro;
            var usuarioRegistroOriginal = existingEmpresa.UsuarioRegistro;

            _mapper.Map(updateDto, existingEmpresa);

            // Restaurar valores que no deben cambiar
            existingEmpresa.FechaRegistro = fechaRegistroOriginal;
            existingEmpresa.UsuarioRegistro = usuarioRegistroOriginal;

            var updatedEmpresa = await _empresaRepository.UpdateAsync(existingEmpresa);

            // Obtener la empresa actualizada con las relaciones
            var empresaWithRelations = await _empresaRepository.GetByIdAsync(updatedEmpresa.Id);
            return _mapper.Map<EmpresaDto>(empresaWithRelations);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (!await _empresaRepository.ExistsAsync(id))
            {
                throw new KeyNotFoundException($"No se encontró la empresa con ID {id}");
            }

            return await _empresaRepository.DeleteAsync(id);
        }

        public async Task<bool> ExistsByNumDocYPaisAsync(string numDocContribuyente, int? idPais)
        {
            return await _empresaRepository.ExistsByNumDocYPaisAsync(numDocContribuyente, idPais);
        }
    }
}
