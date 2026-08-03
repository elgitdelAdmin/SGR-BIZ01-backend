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
    public class ConsultorService : IConsultorService
    {
        private readonly IConsultorRepository _consultorRepository;
        private readonly IPersonaRepository _personaRepository;
        private readonly IPersonaService _personaService;
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IConsultorFrenteSubFrenteRepository _consultorFrenteSubFrenteRepository;
        private readonly IFrenteRepository _frenteRepository;
        private readonly ISubFrenteRepository _subFrenteRepository;
        private readonly IAuthService _userService;

        public ConsultorService(
            IUserRepository userRepository,
            IPersonaRepository personaRepository,
            IPersonaService personaService,
            ITokenService tokenService,
            IMapper mapper,
            IConsultorRepository consultorRepository,
            IConsultorFrenteSubFrenteRepository consultorFrenteSubFrenteRepository,
            IFrenteRepository frenteRepository,
            ISubFrenteRepository subFrenteRepository,
            IAuthService userService
            )
        {
            _consultorRepository = consultorRepository;
            _personaRepository = personaRepository;
            _personaService = personaService;
            _userRepository = userRepository;
            _tokenService = tokenService;
            _mapper = mapper;
            _consultorFrenteSubFrenteRepository = consultorFrenteSubFrenteRepository;
            _frenteRepository = frenteRepository;
            _subFrenteRepository = subFrenteRepository;
            _userService = userService;
        }

        public async Task<IEnumerable<ConsultorDto>> GetAllAsync()
        {
            var consultores = await _consultorRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ConsultorDto>>(consultores);
        }

        public async Task<ConsultorDto> GetByIdAsync(int id)
        {
            var consultor = await _consultorRepository.GetByIdAsync(id);
            if (consultor == null)
                return null;

            return _mapper.Map<ConsultorDto>(consultor);
        }

        public async Task<ConsultorDto> GetByIdUserAsync(int iduser)
        {
            var consultor = await _consultorRepository.GetByIdUserAsync(iduser);
            if (consultor == null)
                return null;

            return _mapper.Map<ConsultorDto>(consultor);
        }
        public async Task<IEnumerable<ConsultorDto>> GetByNumDocContribuyenteSocioAsync(string numDocContribuyente)
        {
            var consultor = await _consultorRepository.GetByNumDocContribuyenteSocioAsync(numDocContribuyente);
            if (consultor == null)
                return null;

            return _mapper.Map<IEnumerable<ConsultorDto>>(consultor);
        }

        public async Task<ConsultorDto> UpdateAsync(int id, ConsultorDto consultorDto)
        {
            // Verificar que el consultor existe
            if (!await _consultorRepository.ExistsAsync(id))
                throw new InvalidOperationException($"No existe un consultor con ID {id}");

            consultorDto.Id = id;

            // Obtener el consultor existente
            var consultorExistente = await _consultorRepository.GetByIdAsync(id);
            
            // Actualizar delegando a la entidad (Rich Domain Model)
            consultorExistente.ActualizarDatosBasicos(
                consultorDto.IdNivelExperiencia,
                consultorDto.IdModalidadLaboral,
                consultorDto.IdSocio,
                consultorDto.UsuarioActualizacion
            );

            var consultorActualizado = await _consultorRepository.UpdateAsync(consultorExistente);

            // Actualizar datos de la persona si se incluyen
            if (consultorDto.Persona != null && consultorExistente.Persona != null)
            {
                var personaDto = new UpdatePersonaDto
                {
                    Nombres = consultorDto.Persona.Nombres,
                    ApellidoPaterno = consultorDto.Persona.ApellidoPaterno,
                    ApellidoMaterno = consultorDto.Persona.ApellidoMaterno,
                    NumeroDocumento = consultorDto.Persona.NumeroDocumento,
                    TipoDocumento = consultorDto.Persona.TipoDocumento,
                    Telefono = consultorDto.Persona.Telefono,
                    Telefono2 = consultorDto.Persona.Telefono2,
                    Correo = consultorDto.Persona.Correo,
                    Direccion = consultorDto.Persona.Direccion,
                    FechaNacimiento = DateTime.SpecifyKind((DateTime)consultorDto.Persona.FechaNacimiento, DateTimeKind.Local),
                    UsuarioActualizacion = consultorDto.UsuarioActualizacion
                };
                await _personaService.ValidateUpdateAsync(personaDto);
            }

            // Procesar especializaciones delegando validación al Dominio
            await ActualizarEspecializacionesAsync(consultorExistente, consultorDto.Especializaciones ?? new List<ConsultorFrenteSubFrenteDto>());

            // Obtener el consultor completo con sus relaciones
            var consultorCompleto = await _consultorRepository.GetByIdAsync(consultorActualizado.Id);

            return _mapper.Map<ConsultorDto>(consultorCompleto);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // Validar que el gestor exista
            var consultor = await _consultorRepository.GetByIdAsync(id);
            if (consultor != null)
            {
                await _userRepository.DeleteUserAsync(consultor.IdUser); // <-- AWAIT AQUÍ
            }
            else
            {
                throw new InvalidOperationException($"No se encontró el consultor con ID {id}");
            }

            return await _consultorRepository.DeleteAsync(id);
        }

        #region Métodos privados para manejo de especializaciones

        private async Task ProcesarEspecializacionesAsync(Consultor consultor, IEnumerable<ConsultorFrenteSubFrenteDto> especializacionesDto)
        {
            var nuevasEspecializaciones = especializacionesDto.Select(dto => new ConsultorFrenteSubFrente
            {
                ConsultorId = consultor.Id,
                IdFrente = dto.IdFrente,
                IdSubFrente = dto.IdSubFrente,
                IdNivelExperiencia = dto.IdNivelExperiencia,
                EsCertificado = dto.EsCertificado
            }).ToList();

            // 1. Delegar validación de reglas de negocio a la entidad de Dominio
            consultor.ValidarEspecializacionesNuevas(nuevasEspecializaciones);

            // 2. Persistir
            foreach (var especializacion in nuevasEspecializaciones)
            {
                await _consultorFrenteSubFrenteRepository.CreateAsync(especializacion);
            }
        }

        private async Task ActualizarEspecializacionesAsync(Consultor consultor, IEnumerable<ConsultorFrenteSubFrenteDto> nuevasEspecializacionesDto)
        {
            // Obtener especializaciones actuales para el agregado
            var especializacionesActuales = await _consultorFrenteSubFrenteRepository.GetByConsultorIdAsync(consultor.Id);
            consultor.ConsultorFrenteSubFrente = especializacionesActuales.ToList();

            var nuevasEspecializaciones = nuevasEspecializacionesDto.Select(dto => new ConsultorFrenteSubFrente
            {
                ConsultorId = consultor.Id,
                IdFrente = dto.IdFrente,
                IdSubFrente = dto.IdSubFrente,
                IdNivelExperiencia = dto.IdNivelExperiencia,
                EsCertificado = dto.EsCertificado
            }).ToList();

            // 1. Delegar validación de reglas de negocio a la entidad de Dominio
            consultor.ValidarEspecializacionesNuevas(nuevasEspecializaciones);

            // 2. Verificar si hay diferencias utilizando la lógica encapsulada en el Dominio
            if (consultor.EspecializacionesSonDiferentes(nuevasEspecializaciones))
            {
                // Desactivar todas las especializaciones actuales del consultor
                await _consultorFrenteSubFrenteRepository.DeleteByConsultorIdAsync(consultor.Id);

                // Registrar las nuevas especializaciones
                if (nuevasEspecializaciones.Any())
                {
                    foreach (var especializacion in nuevasEspecializaciones)
                    {
                        await _consultorFrenteSubFrenteRepository.CreateAsync(especializacion);
                    }
                }
            }
        }
        #endregion
    }
}
