using AutoMapper;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Constants;
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
        private readonly IPersonaService _personaService;
        private readonly IAuthService _userService;
        private readonly IMapper _mapper;

        public EmpresaService(
            IEmpresaRepository empresaRepository,
            IPersonaRepository personaRepository,
            IPersonaService personaService,
            IAuthService userService,
            IMapper mapper)
        {
            _empresaRepository = empresaRepository;
            _personaRepository = personaRepository;
            _personaService = personaService;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<EmpresaDto>> GetAllAsync()
        {
            var empresas = await _empresaRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<EmpresaDto>>(empresas);
        }
        public async Task<IEnumerable<EmpresaDto>> GetByIdSocio(int idSocio)
        {
            var empresas = await _empresaRepository.GetByIdSocio(idSocio);
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
        public async Task<EmpresaDto> GetByIdUserAsync(int iduser)
        {
            var empresa = await _empresaRepository.GetByIdUserAsync(iduser);
            if (empresa == null)
                return null;
            return _mapper.Map<EmpresaDto>(empresa);
        }
        public async Task<PersonaConUsuariosEmpresaDto> GetPersonaResponsableByTipoNumDoc(int idTipoDocumento, string numeroDocumento)
        {
            if (string.IsNullOrEmpty(numeroDocumento))
            {
                throw new InvalidOperationException("Se debe proporcionar un número de documento válido para la persona responsable");
            }

            var persona = await _personaRepository.GetByResponsableTipoNumDocumentoAsync(idTipoDocumento, numeroDocumento, AppConstants.Roles.Empresa);

            if (persona == null)
            {
                throw new InvalidOperationException("No se encontró una persona con el documento proporcionado");
            }
            var personaDto = _mapper.Map<PersonaConUsuariosEmpresaDto>(persona);
            return personaDto;
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
            var personaExistente = await _personaRepository.GetByTipoNumDocumentoAsync(createDto.Persona.TipoDocumento, createDto.Persona.NumeroDocumento);

            if (personaExistente == null)
            {
                throw new InvalidOperationException("No se encontró una persona con el número de documento proporcionado");
            }

            PersonaDto persona = new PersonaDto();
            // Actualizar datos de la persona si se incluyen
            if (createDto.Persona != null)
            {
                var personaDto = new UpdatePersonaDto
                {
                    Nombres = createDto.Persona.Nombres,
                    ApellidoPaterno = createDto.Persona.ApellidoPaterno,
                    ApellidoMaterno = createDto.Persona.ApellidoMaterno,
                    NumeroDocumento = createDto.Persona.NumeroDocumento,
                    TipoDocumento = createDto.Persona.TipoDocumento,
                    Telefono = createDto.Persona.Telefono,
                    Telefono2 = createDto.Persona.Telefono2,
                    Correo = createDto.Persona.Correo,
                    Direccion = createDto.Persona.Direccion,
                    FechaNacimiento = DateTime.SpecifyKind((DateTime)createDto.Persona.FechaNacimiento, DateTimeKind.Local),
                    UsuarioActualizacion = createDto.UsuarioRegistro
                };
                persona = await _personaService.ValidateUpdateAsync(personaDto);
            }
            RolDto rol = await _userService.GetRolByCodigoAsync(AppConstants.Roles.Empresa);
            //UserDto usuario = await _userService.GetByIdSocioIdRolIdAsync(createDto.IdSocio, rol.Id, persona.Id);

            // Mapear el DTO a la entidad Empresa
            var empresa = _mapper.Map<Empresa>(createDto);
            empresa.FechaRegistro = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

            // Asignar el ID de la persona responsable (asumiendo que tu entidad Empresa tiene esta propiedad)
            empresa.IdPersonaResponsable = persona.Id;
            empresa.IdUser = createDto.IdUser;
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

            // Variable para almacenar el ID de la persona responsable
            int personaId = existingEmpresa.IdPersonaResponsable; // Mantener el ID actual por defecto

            var personaExistente = await _personaRepository.GetByTipoNumDocumentoAsync(updateDto.Persona.TipoDocumento, updateDto.Persona.NumeroDocumento);
            if (personaExistente == null)
            {
                throw new InvalidOperationException("No se encontró una persona con el número de documento proporcionado");
            }

            PersonaDto persona = new PersonaDto();
            // Actualizar datos de la persona si se incluyen
            if (updateDto.Persona != null)
            {
                var personaDto = new UpdatePersonaDto
                {
                    Nombres = updateDto.Persona.Nombres,
                    ApellidoPaterno = updateDto.Persona.ApellidoPaterno,
                    ApellidoMaterno = updateDto.Persona.ApellidoMaterno,
                    NumeroDocumento = updateDto.Persona.NumeroDocumento,
                    TipoDocumento = updateDto.Persona.TipoDocumento,
                    Telefono = updateDto.Persona.Telefono,
                    Telefono2 = updateDto.Persona.Telefono2,
                    Correo = updateDto.Persona.Correo,
                    Direccion = updateDto.Persona.Direccion,
                    FechaNacimiento = DateTime.SpecifyKind((DateTime)updateDto.Persona.FechaNacimiento, DateTimeKind.Local),
                    UsuarioActualizacion = updateDto.UsuarioModificacion
                };
                persona = await _personaService.ValidateUpdateAsync(personaDto);
                personaId = persona.Id;
            }
            RolDto rol = await _userService.GetRolByCodigoAsync(AppConstants.Roles.Empresa);
            UserDto usuario = await _userService.GetByIdSocioIdRolIdAsync(updateDto.IdSocio, rol.Id, persona.Id);

            // Mapear el DTO a la entidad existente
            _mapper.Map(updateDto, existingEmpresa);

            // Restaurar valores que no deben cambiar
            existingEmpresa.FechaRegistro = DateTime.SpecifyKind(fechaRegistroOriginal, DateTimeKind.Local) ;
            existingEmpresa.UsuarioRegistro = usuarioRegistroOriginal;

            existingEmpresa.FechaModificacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            existingEmpresa.UsuarioModificacion = updateDto.UsuarioModificacion;

            // Asignar el ID de la persona responsable (puede ser el mismo o uno nuevo)
            existingEmpresa.IdPersonaResponsable = personaId;
            existingEmpresa.IdUser = updateDto.IdUser;

            // Actualizar la empresa
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
