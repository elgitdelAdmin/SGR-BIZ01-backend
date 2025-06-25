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

        public async Task<PersonaDto> ValidateCreateUpdate(CreatePersonaDto createPersonaDto)
        {
            PersonaDto personaDto = new PersonaDto();
            // Validar y procesar información de persona
            if (createPersonaDto != null)
            {
                if (!string.IsNullOrEmpty(createPersonaDto.NumeroDocumento))
                {
                    // Buscar persona existente por tipo de documento y número
                    var personaExistente = await _personaRepository.GetByTipoNumDocumentoAsync(createPersonaDto.TipoDocumento,createPersonaDto.NumeroDocumento);

                    if (personaExistente != null)
                    {
                        // Si la persona existe, actualizarla con los nuevos datos
                        personaExistente.Nombres = createPersonaDto.Nombres;
                        personaExistente.ApellidoMaterno = createPersonaDto.ApellidoMaterno;
                        personaExistente.ApellidoPaterno = createPersonaDto.ApellidoPaterno;
                        personaExistente.Telefono = createPersonaDto.Telefono;
                        personaExistente.Telefono2 = createPersonaDto.Telefono2;
                        personaExistente.Correo = createPersonaDto.Correo;
                        personaExistente.Direccion = createPersonaDto.Direccion;
                        personaExistente.FechaNacimiento = createPersonaDto.FechaNacimiento.HasValue
                            ? DateTime.SpecifyKind(createPersonaDto.FechaNacimiento.Value, DateTimeKind.Local)
                            : null;
                        personaExistente.FechaActualizacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

                        var personaActualizada = await _personaRepository.UpdateAsync(personaExistente);
                        personaDto = _mapper.Map<PersonaDto>(personaActualizada);
                    }
                    else
                    {
                        // Si la persona no existe, crearla
                        var nuevaPersona = _mapper.Map<Persona>(createPersonaDto);
                        nuevaPersona.FechaNacimiento = nuevaPersona.FechaNacimiento.HasValue
                            ? DateTime.SpecifyKind(nuevaPersona.FechaNacimiento.Value, DateTimeKind.Local)
                            : null;
                        nuevaPersona.FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                        nuevaPersona.FechaActualizacion = null;
                        nuevaPersona.Activo = true;

                        var personaCreada = await _personaRepository.CreateAsync(nuevaPersona);
                        personaDto = _mapper.Map<PersonaDto>(personaCreada) ;
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
            return personaDto;
        }
        public async Task<PersonaDto> ValidateUpdateAsync(UpdatePersonaDto updatePersonaDto)
        {
            if (updatePersonaDto == null)
                throw new InvalidOperationException("Se debe proporcionar información de la persona para la actualización.");

            if (string.IsNullOrEmpty(updatePersonaDto.NumeroDocumento))
                throw new InvalidOperationException("Se debe proporcionar un número de documento válido para la persona.");

            var personaExistente = await _personaRepository.GetByTipoNumDocumentoAsync(updatePersonaDto.TipoDocumento, updatePersonaDto.NumeroDocumento);

            if (personaExistente == null)
                throw new InvalidOperationException("No se encontró una persona registrada con el tipo y número de documento proporcionado.");

            // Actualizar campos
            personaExistente.TipoDocumento = updatePersonaDto.TipoDocumento;
            personaExistente.NumeroDocumento = updatePersonaDto.NumeroDocumento;
            personaExistente.Nombres = updatePersonaDto.Nombres;
            personaExistente.ApellidoMaterno = updatePersonaDto.ApellidoMaterno;
            personaExistente.ApellidoPaterno = updatePersonaDto.ApellidoPaterno;
            personaExistente.Telefono = updatePersonaDto.Telefono;
            personaExistente.Telefono2 = updatePersonaDto.Telefono2;
            personaExistente.Correo = updatePersonaDto.Correo;
            personaExistente.Direccion = updatePersonaDto.Direccion;
            personaExistente.FechaNacimiento = updatePersonaDto.FechaNacimiento.HasValue
                ? DateTime.SpecifyKind(updatePersonaDto.FechaNacimiento.Value, DateTimeKind.Local)
                : null;
            personaExistente.FechaActualizacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            personaExistente.UsuarioActualizacion = updatePersonaDto.UsuarioActualizacion;
            // Guardar cambios
            var personaActualizada = await _personaRepository.UpdateAsync(personaExistente);

            // Mapear resultado a DTO
            return _mapper.Map<PersonaDto>(personaActualizada);
        }

    }
}
