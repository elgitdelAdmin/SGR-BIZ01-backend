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
    public class GestorService : IGestorService
    {
        private readonly IGestorRepository _gestorRepository;
        private readonly IPersonaRepository _personaRepository;
        private readonly IGestorFrenteSubFrenteRepository _gestorFrenteSubFrenteRepository;
        private readonly IMapper _mapper;

        public GestorService(IGestorRepository gestorRepository,
                            IPersonaRepository personaRepository,
                            IGestorFrenteSubFrenteRepository gestorFrenteSubFrenteRepository,
                            IMapper mapper)
        {
            _gestorRepository = gestorRepository;
            _personaRepository = personaRepository;
            _gestorFrenteSubFrenteRepository = gestorFrenteSubFrenteRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<GestorDto>> GetAllAsync()
        {
            var gestores = await _gestorRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<GestorDto>>(gestores);
        }

        public async Task<GestorDto?> GetByIdAsync(int id)
        {
            var gestor = await _gestorRepository.GetByIdAsync(id);
            return gestor != null ? _mapper.Map<GestorDto>(gestor) : null;
        }

        public async Task<GestorDto> CreateAsync(CreateGestorDto createGestorDto)
        {
            // Validar y obtener/crear persona
            var persona = await ValidarYObtenerPersonaAsync(createGestorDto);

            // Validar que la persona no esté asignada como gestor
            if (await _gestorRepository.ExistsByPersonaIdAsync(persona.Id))
            {
                throw new InvalidOperationException("La persona ya está asignada como gestor");
            }

            // Crear gestor
            var gestor = new Gestor
            {
                PersonaId = persona.Id,
                IdNivelExperiencia = createGestorDto.IdNivelExperiencia,
                IdModalidadLaboral = createGestorDto.IdModalidadLaboral,
                IdSocio = createGestorDto.IdSocio,
                UsuarioCreacion = createGestorDto.UsuarioCreacion,
                FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
                Activo = true
            };

            var gestorCreated = await _gestorRepository.CreateAsync(gestor);

            // Crear GestorFrenteSubFrente
            if (createGestorDto.FrentesSubFrente.Any())
            {
                await CrearGestorFrenteSubFrenteAsync(gestorCreated.Id, createGestorDto.FrentesSubFrente);
            }

            // Obtener el gestor completo para retornar
            var gestorCompleto = await _gestorRepository.GetByIdAsync(gestorCreated.Id);
            return _mapper.Map<GestorDto>(gestorCompleto);
        }

        public async Task<GestorDto> UpdateAsync(UpdateGestorDto updateGestorDto)
        {
            // Validar que el gestor exista
            var gestorExistente = await _gestorRepository.GetByIdAsync(updateGestorDto.Id);
            if (gestorExistente == null)
            {
                throw new InvalidOperationException($"No se encontró el gestor con ID {updateGestorDto.Id}");
            }

            // Validar y obtener/actualizar persona
            var persona = await ValidarYActualizarPersonaAsync(updateGestorDto, gestorExistente.PersonaId);

            // Validar que la persona no esté asignada a otro gestor
            if (persona.Id != gestorExistente.PersonaId &&
                await _gestorRepository.ExistsByPersonaIdAsync(persona.Id, updateGestorDto.Id))
            {
                throw new InvalidOperationException("La persona ya está asignada a otro gestor");
            }

            // Actualizar gestor
            gestorExistente.PersonaId = persona.Id;
            gestorExistente.IdNivelExperiencia = updateGestorDto.IdNivelExperiencia;
            gestorExistente.IdModalidadLaboral = updateGestorDto.IdModalidadLaboral;
            gestorExistente.UsuarioActualizacion = updateGestorDto.UsuarioActualizacion;
            gestorExistente.FechaActualizacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

            var gestorUpdated = await _gestorRepository.UpdateAsync(gestorExistente);

            // Gestionar GestorFrenteSubFrente
            await GestionarGestorFrenteSubFrenteAsync(gestorUpdated.Id, updateGestorDto.FrentesSubFrente);

            // Obtener el gestor completo para retornar
            var gestorCompleto = await _gestorRepository.GetByIdAsync(gestorUpdated.Id);
            return _mapper.Map<GestorDto>(gestorCompleto);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // Validar que el gestor exista
            if (!await _gestorRepository.ExistsByIdAsync(id))
            {
                throw new InvalidOperationException($"No se encontró el gestor con ID {id}");
            }

            return await _gestorRepository.DeleteAsync(id);
        }

        private async Task<Persona> ValidarYObtenerPersonaAsync(CreateGestorDto createGestorDto)
        {
            // Buscar persona existente por documento
            var personaExistente = await _personaRepository.GetByTipoNumDocumentoAsync(createGestorDto.TipoDocumento,createGestorDto.NumeroDocumento);

            if (personaExistente != null)
            {
                return personaExistente;
            }

            // Crear nueva persona
            var nuevaPersona = new Persona
            {
                Nombres = createGestorDto.Nombres,
                ApellidoPaterno = createGestorDto.ApellidoPaterno,
                ApellidoMaterno = createGestorDto.ApellidoMaterno,
                NumeroDocumento = createGestorDto.NumeroDocumento,
                TipoDocumento = createGestorDto.TipoDocumento,
                Telefono = createGestorDto.Telefono,
                Telefono2 = createGestorDto.Telefono2,
                Correo = createGestorDto.Correo,
                Direccion = createGestorDto.Direccion,
                FechaNacimiento = DateTime.SpecifyKind(createGestorDto.FechaNacimiento, DateTimeKind.Local),
                FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
                Activo = true
            };

            return await _personaRepository.CreateAsync(nuevaPersona);
        }

        private async Task<Persona> ValidarYActualizarPersonaAsync(UpdateGestorDto updateGestorDto, int personaIdActual)
        {
            // Buscar persona por documento
            var personaPorDocumento = await _personaRepository.GetByTipoNumDocumentoAsync(updateGestorDto.TipoDocumento,updateGestorDto.NumeroDocumento);

            if (personaPorDocumento != null && personaPorDocumento.Id != personaIdActual)
            {
                // La persona ya existe y es diferente a la actual
                return personaPorDocumento;
            }

            // Actualizar persona actual
            var personaActual = await _personaRepository.GetByIdAsync(personaIdActual);
            if (personaActual != null)
            {
                personaActual.Nombres = updateGestorDto.Nombres;
                personaActual.ApellidoPaterno = updateGestorDto.ApellidoPaterno;
                personaActual.ApellidoMaterno = updateGestorDto.ApellidoMaterno;
                personaActual.NumeroDocumento = updateGestorDto.NumeroDocumento;
                personaActual.TipoDocumento = updateGestorDto.TipoDocumento;
                personaActual.Telefono = updateGestorDto.Telefono;
                personaActual.Telefono2 = updateGestorDto.Telefono2;
                personaActual.Correo = updateGestorDto.Correo;
                personaActual.Direccion = updateGestorDto.Direccion;
                personaActual.FechaNacimiento = updateGestorDto.FechaNacimiento;
                personaActual.FechaActualizacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

                return await _personaRepository.UpdateAsync(personaActual);
            }

            throw new InvalidOperationException("No se encontró la persona asociada al gestor");
        }

        private async Task CrearGestorFrenteSubFrenteAsync(int IdGestor, List<CreateGestorFrenteSubFrenteDto> frentesSubFrente)
        {
            foreach (var item in frentesSubFrente)
            {
                var gestorFrenteSubFrente = new GestorFrenteSubFrente
                {
                    IdGestor = IdGestor,
                    IdFrente = item.IdFrente,
                    IdSubFrente = item.IdSubFrente,
                    IdNivelExperiencia = item.IdNivelExperiencia,
                    EsCertificado = item.EsCertificado,
                    FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
                    UsuarioCreacion = item.UsuarioCreacion,
                    Activo = true
                };

                await _gestorFrenteSubFrenteRepository.CreateAsync(gestorFrenteSubFrente);
            }
        }

        private async Task GestionarGestorFrenteSubFrenteAsync(int IdGestor, List<CreateGestorFrenteSubFrenteDto> nuevosFrentesSubFrente)
        {
            // Obtener los frentes/subfrentes actuales
            var frentesActuales = await _gestorFrenteSubFrenteRepository.GetByGestorIdAsync(IdGestor);

            // Comparar las listas para ver si han cambiado
            var hanCambiado = HanCambiadoFrentesSubFrente(frentesActuales, nuevosFrentesSubFrente);

            if (hanCambiado)
            {
                // Desactivar todos los registros actuales
                await _gestorFrenteSubFrenteRepository.DeactivateByGestorIdAsync(IdGestor);

                // Crear los nuevos registros
                if (nuevosFrentesSubFrente.Any())
                {
                    await CrearGestorFrenteSubFrenteAsync(IdGestor, nuevosFrentesSubFrente);
                }
            }
        }

        private bool HanCambiadoFrentesSubFrente(IEnumerable<GestorFrenteSubFrente> actuales, List<CreateGestorFrenteSubFrenteDto> nuevos)
        {
            var actualesList = actuales.ToList();

            // Si las cantidades son diferentes, han cambiado
            if (actualesList.Count != nuevos.Count)
                return true;

            // Comparar cada elemento
            foreach (var actual in actualesList)
            {
                var existe = nuevos.Any(n =>
                    n.IdFrente == actual.IdFrente &&
                    n.IdSubFrente == actual.IdSubFrente &&
                    n.IdNivelExperiencia == actual.IdNivelExperiencia &&
                    n.EsCertificado == actual.EsCertificado);

                if (!existe)
                    return true;
            }

            return false;
        }
    }
}