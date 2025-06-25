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

        public ConsultorService(
            IUserRepository userRepository,
            IPersonaRepository personaRepository,
            IPersonaService personaService,
            ITokenService tokenService,
            IMapper mapper,
            IConsultorRepository consultorRepository,
            IConsultorFrenteSubFrenteRepository consultorFrenteSubFrenteRepository,
            IFrenteRepository frenteRepository,
            ISubFrenteRepository subFrenteRepository
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

        public async Task<ConsultorDto> CreateAsync(ConsultorDto consultorDto)
        {
            try
            {
                // Validar y actalizar/crear persona
                var personaDto = new CreatePersonaDto
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
                };
                PersonaDto persona = await _personaService.ValidateCreateUpdate(personaDto);
      
                // Validar que la persona no esté asignada como gestor
                if (await _consultorRepository.ExistsByPersonaIdAsync(persona.Id))
                {
                    throw new InvalidOperationException("La persona ya está asignada como gestor");
                }

                // Mapear el DTO a la entidad
                var consultor = _mapper.Map<Consultor>(consultorDto);
                consultor.PersonaId = persona.Id;
                consultor.FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                consultor.FechaActualizacion = consultor.FechaActualizacion.HasValue == true
                    ? DateTime.SpecifyKind(consultor.FechaActualizacion.Value, DateTimeKind.Local)
                    : null;
                consultor.Activo = true;

                // Crear el consultor
                var consultorCreado = await _consultorRepository.CreateAsync(consultor);

                // Procesar especializaciones si existen
                if (consultorDto.Especializaciones != null && consultorDto.Especializaciones.Any())
                {
                    await ProcesarEspecializacionesAsync(consultorCreado.Id, consultorDto.Especializaciones);
                }

                // Obtener el consultor completo con sus relaciones
                var consultorCompleto = await _consultorRepository.GetByIdAsync(consultorCreado.Id);

                return _mapper.Map<ConsultorDto>(consultorCompleto);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ConsultorDto> UpdateAsync(int id, ConsultorDto consultorDto)
        {
            // Verificar que el consultor existe
            if (!await _consultorRepository.ExistsAsync(id))
                throw new InvalidOperationException($"No existe un consultor con ID {id}");

            consultorDto.Id = id;

            // Obtener el consultor existente
            var consultorExistente = await _consultorRepository.GetByIdAsync(id);
            // Actualizar el consultor
            var consultor = _mapper.Map<Consultor>(consultorDto);
            consultor.FechaCreacion = DateTime.SpecifyKind(consultorExistente.FechaCreacion, DateTimeKind.Local);
            consultor.FechaActualizacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            consultor.UsuarioActualizacion = consultorDto.UsuarioActualizacion;
            var consultorActualizado = await _consultorRepository.UpdateAsync(consultor);

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

            // Procesar especializaciones
            await ActualizarEspecializacionesAsync(id, consultorDto.Especializaciones ?? new List<ConsultorFrenteSubFrenteDto>());

            // Obtener el consultor completo con sus relaciones
            var consultorCompleto = await _consultorRepository.GetByIdAsync(consultorActualizado.Id);

            return _mapper.Map<ConsultorDto>(consultorCompleto);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (!await _consultorRepository.ExistsAsync(id))
                return false;

            // Eliminar especializaciones primero
            await _consultorFrenteSubFrenteRepository.DeleteByConsultorIdAsync(id);

            // Eliminar el consultor
            return await _consultorRepository.DeleteAsync(id);
        }

        #region Métodos privados para manejo de especializaciones

        //private async Task ValidarEspecializacionesAsync(IEnumerable<ConsultorFrenteSubFrenteDto> especializaciones)
        //{
        //    foreach (var especializacion in especializaciones)
        //    {
        //        // Validar que el frente existe
        //        if (!await _frenteRepository.ExistsAsync(especializacion.IdFrente))
        //            throw new InvalidOperationException($"El frente con ID {especializacion.IdFrente} no existe");

        //        // Validar que el subfrente existe
        //        if (!await _subFrenteRepository.ExistsAsync(especializacion.IdSubFrente))
        //            throw new InvalidOperationException($"El subfrente con ID {especializacion.IdSubFrente} no existe");

        //        // Validar que el subfrente pertenece al frente
        //        //if (!await _subFrenteRepository.BelongsToFrenteAsync(especializacion.IdSubFrente, especializacion.IdFrente))
        //        //    throw new InvalidOperationException($"El subfrente con ID {especializacion.IdSubFrente} no pertenece al frente con ID {especializacion.IdFrente}");
        //    }

        //    // Validar que no hay duplicados en la lista
        //    var duplicados = especializaciones
        //        .GroupBy(e => new { e.IdFrente, e.IdSubFrente })
        //        .Where(g => g.Count() > 1)
        //        .Select(g => g.Key);

        //    if (duplicados.Any())
        //    {
        //        var duplicadosStr = string.Join(", ", duplicados.Select(d => $"Frente:{d.IdFrente}-SubFrente:{d.IdSubFrente}"));
        //        throw new InvalidOperationException($"Se encontraron especializaciones duplicadas: {duplicadosStr}");
        //    }
        //}

        private async Task ProcesarEspecializacionesAsync(int consultorId, IEnumerable<ConsultorFrenteSubFrenteDto> especializaciones)
        {
            foreach (var especializacionDto in especializaciones)
            {
                // Verificar que no exista ya esta combinación
                if (await _consultorFrenteSubFrenteRepository.ExistsAsync(consultorId, 
                    especializacionDto.IdFrente, 
                    especializacionDto.IdSubFrente, 
                    especializacionDto.IdNivelExperiencia,
                    especializacionDto.EsCertificado))
                    continue; // Saltar si ya existe

                var especializacion = new ConsultorFrenteSubFrente
                {
                    ConsultorId = consultorId,
                    IdFrente = especializacionDto.IdFrente,
                    IdSubFrente = especializacionDto.IdSubFrente,
                    IdNivelExperiencia = especializacionDto.IdNivelExperiencia,
                    EsCertificado = especializacionDto.EsCertificado,
                    FechaCreacion = DateTime.Now,
                    Activo = true
                };

                await _consultorFrenteSubFrenteRepository.CreateAsync(especializacion);
            }
        }

        private async Task ActualizarEspecializacionesAsync(int consultorId, IEnumerable<ConsultorFrenteSubFrenteDto> nuevasEspecializaciones)
        {
            // Obtener especializaciones actuales
            var especializacionesActuales = await _consultorFrenteSubFrenteRepository.GetByConsultorIdAsync(consultorId);

            // Verificar si hay diferencias
            if (HayDiferencias(especializacionesActuales, nuevasEspecializaciones))
            {
                // Desactivar todas las especializaciones actuales del consultor
                await _consultorFrenteSubFrenteRepository.DeleteByConsultorIdAsync(consultorId);

                // Registrar las nuevas especializaciones
                if (nuevasEspecializaciones.Any())
                {
                    await ProcesarEspecializacionesAsync(consultorId, nuevasEspecializaciones);
                }
            }
        }
        private bool HayDiferencias(IEnumerable<ConsultorFrenteSubFrente> actuales, IEnumerable<ConsultorFrenteSubFrenteDto> nuevas)
        {
            // Convertir a listas para facilitar comparación
            var actualesList = actuales.ToList();
            var nuevasList = nuevas.ToList();

            // Si tienen diferente cantidad, hay diferencias
            if (actualesList.Count != nuevasList.Count)
                return true;

            // Comparar cada especialización actual con las nuevas
            foreach (var actual in actualesList)
            {
                var existe = nuevasList.Any(nueva =>
                    nueva.IdFrente == actual.IdFrente &&
                    nueva.IdSubFrente == actual.IdSubFrente &&
                    nueva.IdNivelExperiencia == actual.IdNivelExperiencia &&
                    nueva.EsCertificado == actual.EsCertificado);

                if (!existe)
                    return true;
            }

            // Si llegamos aquí, no hay diferencias
            return false;
        }
        #endregion
    }
}
