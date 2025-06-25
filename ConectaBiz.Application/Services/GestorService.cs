using AutoMapper;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Services
{
    public class GestorService : IGestorService
    {
        private readonly IGestorRepository _gestorRepository;
        private readonly IPersonaService _personaService;
        private readonly IGestorFrenteSubFrenteRepository _gestorFrenteSubFrenteRepository;
        private readonly IMapper _mapper;

        public GestorService(IGestorRepository gestorRepository,
                            IPersonaService personaService,
                            IGestorFrenteSubFrenteRepository gestorFrenteSubFrenteRepository,
                            IMapper mapper)
        {
            _gestorRepository = gestorRepository;
            _personaService = personaService;
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
        public async Task<GestorDto?> GetByIdUserAsync(int iduser)
        {
            var gestor = await _gestorRepository.GetByIdUserAsync(iduser);
            return gestor != null ? _mapper.Map<GestorDto>(gestor) : null;
        }

        public async Task<GestorDto> CreateAsync(CreateGestorDto createGestorDto)
        {
            // Validar y actalizar/crear persona
            var personaDto = new CreatePersonaDto
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
            };
            PersonaDto persona = await _personaService.ValidateCreateUpdate(personaDto);
            if (persona == null || persona.Id == 0)
            {
                throw new InvalidOperationException("No se pudo validar o crear la persona.");
            }

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

        public async Task<GestorDto> UpdateAsync(int id, UpdateGestorDto updateGestorDto)
        {
            // Validar que el gestor exista
            var gestorExistente = await _gestorRepository.GetByIdAsync(id);
            if (gestorExistente == null)
            {
                throw new InvalidOperationException($"No se encontró el gestor con ID {id}");
            }
            var personaDto = new UpdatePersonaDto
            {
                Nombres = updateGestorDto.Nombres,
                ApellidoPaterno = updateGestorDto.ApellidoPaterno,
                ApellidoMaterno = updateGestorDto.ApellidoMaterno,
                NumeroDocumento = updateGestorDto.NumeroDocumento,
                TipoDocumento = updateGestorDto.TipoDocumento,
                Telefono = updateGestorDto.Telefono,
                Telefono2 = updateGestorDto.Telefono2,
                Correo = updateGestorDto.Correo,
                Direccion = updateGestorDto.Direccion,
                FechaNacimiento = DateTime.SpecifyKind((DateTime)updateGestorDto.FechaNacimiento, DateTimeKind.Local),
                UsuarioActualizacion = updateGestorDto.UsuarioActualizacion
            };
            PersonaDto persona = await _personaService.ValidateUpdateAsync(personaDto);

            // Validar que la persona no esté asignada a otro gestor
            if (persona.Id != gestorExistente.PersonaId &&
                await _gestorRepository.ExistsByPersonaIdAsync(persona.Id, id))
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