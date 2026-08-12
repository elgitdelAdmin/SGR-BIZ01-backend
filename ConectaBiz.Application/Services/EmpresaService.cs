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
        private readonly IGestorService _gestorService;
        private readonly IAuthService _userService;
        private readonly IMapper _mapper;

        public EmpresaService(
            IEmpresaRepository empresaRepository,
            IPersonaRepository personaRepository,
            IPersonaService personaService,
            IGestorService gestorService,
            IAuthService userService,
            IMapper mapper)
        {
            _empresaRepository = empresaRepository;
            _personaRepository = personaRepository;
            _personaService = personaService;
            _gestorService = gestorService;
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
        public async Task<IEnumerable<EmpresaDto>> GetByIdUserIdRolAsync(int idUser, string codRol, int? idSocio = null)
        {
            IEnumerable<EmpresaDto> listadoEmpresas = Enumerable.Empty<EmpresaDto>();
            if (codRol == AppConstants.Roles.SuperAdmin)
            {
                var empresas = await _empresaRepository.GetAllAsync();
                listadoEmpresas = _mapper.Map<IEnumerable<EmpresaDto>>(empresas);
            }
            else if (codRol == AppConstants.Roles.GestorCuenta)
            {
                GestorDto gestorDto = await _gestorService.GetByIdUserAsync(idUser);
                if (gestorDto != null)
                {
                    int socioIdToUse = (idSocio.HasValue && idSocio.Value > 0) ? idSocio.Value : gestorDto.IdSocio;
                    var empresas = await _empresaRepository.GetByIdGestorCuenta(gestorDto.Id, socioIdToUse);
                    listadoEmpresas = _mapper.Map<IEnumerable<EmpresaDto>>(empresas);
                }
            }
            else
            {
                int socioIdToUse = 0;
                if (idSocio.HasValue && idSocio.Value > 0)
                {
                    socioIdToUse = idSocio.Value;
                }
                else
                {
                    UserDto userDto = await _userService.GetByIdAsync(idUser);
                    socioIdToUse = userDto.Socio?.Id ?? 0;
                }

                var empresas = await _empresaRepository.GetByIdSocio(socioIdToUse);
                listadoEmpresas = _mapper.Map<IEnumerable<EmpresaDto>>(empresas);
            }
            return listadoEmpresas;
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
        public async Task<EmpresaDto> GetByNumDocContribuyenteAsync(string numDocContribuyente, string numDocSocio)
        {
            var empresa = await _empresaRepository.GetByNumDocContribuyenteAsync(numDocContribuyente, numDocSocio);
            if (empresa == null)
                return null;
            return _mapper.Map<EmpresaDto>(empresa);
        }
        public async Task<PersonaConUsuariosEmpresaDto> GetPersonaResponsableByTipoNumDoc(int idTipoDocumento, string numeroDocumento)
        {
            if (string.IsNullOrEmpty(numeroDocumento))
            {
                throw new InvalidOperationException("Debe proporcionar un número de documento válido.");
            }

            var persona = await _personaRepository.GetByResponsableTipoNumDocumentoAsync(
                idTipoDocumento,
                numeroDocumento,
                AppConstants.Roles.Empresa
            );

            if (persona == null)
            {
                return null;
            }

            return _mapper.Map<PersonaConUsuariosEmpresaDto>(persona);
        }

        public async Task<EmpresaDto> CreateAsync(CreateEmpresaDto createDto)
        {
            // Validar que no exista una empresa con el mismo SOCIO + PAIS + RUC
            var yaExiste = await _empresaRepository.ExistsByNumDocYPaisAsync(createDto.NumDocContribuyente, createDto.IdPais, createDto.IdSocio);
            if (yaExiste)
            {
                throw new InvalidOperationException($"Ya existe una empresa registrada con el Nro. de documento '{createDto.NumDocContribuyente}' para este socio y país.");
            }

            var personaExistente = await _personaRepository.GetByTipoNumDocumentoAsync(createDto.Persona.TipoDocumento, createDto.Persona.NumeroDocumento);
            if (personaExistente == null)
            {
                throw new InvalidOperationException("No se encontró una persona con el número de documento proporcionado");
            }

            PersonaDto persona = new PersonaDto();
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

            // Uso del Factory Method
            var empresa = Empresa.Crear(
                createDto.RazonSocial,
                createDto.NombreComercial,
                createDto.NumDocContribuyente,
                createDto.IdSocio,
                createDto.IdPais,
                persona.Id,
                createDto.IdUser,
                createDto.CargoResponsable,
                createDto.UsuarioRegistro,
                createDto.CodSgrCsti);

            // Sincronizar gestores en la entidad
            // Construir el diccionario de gestores y sus tipos de ticket
            var dictGestores = new Dictionary<int, List<int>>();
            if (createDto.GestoresAsignados != null && createDto.GestoresAsignados.Any())
            {
                foreach (var g in createDto.GestoresAsignados)
                {
                    dictGestores[g.IdGestor] = g.IdsTiposTicket ?? new List<int>();
                }
            }
            else
            {
                // Fallback para mantener compatibilidad con envíos anteriores o pantallas simples
                var idsFallback = createDto.IdsGestores ?? (createDto.IdGestor.HasValue && createDto.IdGestor.Value > 0 ? new List<int> { createDto.IdGestor.Value } : new List<int>());
                foreach (var idF in idsFallback)
                {
                    dictGestores[idF] = new List<int>();
                }
            }

            var idPrincipal = createDto.IdGestorPrincipal ?? (createDto.IdGestor.HasValue && createDto.IdGestor.Value > 0 ? createDto.IdGestor : null);
            empresa.SincronizarGestores(dictGestores, idPrincipal, createDto.UsuarioRegistro ?? "Sistema");

            // Crear la empresa en BD (guarda la empresa y los gestores trackeados)
            var createdEmpresa = await _empresaRepository.CreateAsync(empresa);

            // Obtener nuevamente para asegurar que vengan las relaciones (por si EF no lo hizo)
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

            var personaExistente = await _personaRepository.GetByTipoNumDocumentoAsync(updateDto.Persona.TipoDocumento, updateDto.Persona.NumeroDocumento);
            if (personaExistente == null)
            {
                throw new InvalidOperationException("No se encontró una persona con el número de documento proporcionado");
            }

            int personaId = existingEmpresa.IdPersonaResponsable;
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
                var persona = await _personaService.ValidateUpdateAsync(personaDto);
                personaId = persona.Id;
            }

            // Uso del método Actualizar del dominio
            existingEmpresa.Actualizar(
                updateDto.RazonSocial,
                updateDto.NombreComercial,
                updateDto.NumDocContribuyente,
                updateDto.Direccion,
                updateDto.Telefono,
                updateDto.Email,
                updateDto.CargoResponsable,
                updateDto.Activo,
                updateDto.IdPais,
                personaId,
                updateDto.IdUser,
                updateDto.UsuarioModificacion);

            // Sincronizar gestores en la entidad
            // Construir el diccionario de gestores y sus tipos de ticket
            var dictGestores = new Dictionary<int, List<int>>();
            if (updateDto.GestoresAsignados != null && updateDto.GestoresAsignados.Any())
            {
                foreach (var g in updateDto.GestoresAsignados)
                {
                    dictGestores[g.IdGestor] = g.IdsTiposTicket ?? new List<int>();
                }
            }
            else
            {
                // Fallback para mantener compatibilidad
                var idsFallback = updateDto.IdsGestores ?? (updateDto.IdGestor.HasValue && updateDto.IdGestor.Value > 0 ? new List<int> { updateDto.IdGestor.Value } : new List<int>());
                foreach (var idF in idsFallback)
                {
                    dictGestores[idF] = new List<int>();
                }
            }

            var idPrincipal = updateDto.IdGestorPrincipal ?? (updateDto.IdGestor.HasValue && updateDto.IdGestor.Value > 0 ? updateDto.IdGestor : null);
            existingEmpresa.SincronizarGestores(dictGestores, idPrincipal, updateDto.UsuarioModificacion ?? "Sistema");

            // EF detectará los cambios
            var updatedEmpresa = await _empresaRepository.UpdateAsync(existingEmpresa);

            return _mapper.Map<EmpresaDto>(updatedEmpresa);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var empresa = await _empresaRepository.GetByIdAsync(id);
            if (empresa == null)
            {
                throw new InvalidOperationException($"No se encontró la empresa con ID {id}");
            }

            if (empresa.IdUser != null)
            {
                await _userService.DeleteUserAsync(empresa.IdUser.Value);
            }

            // Uso del método Desactivar del dominio
            empresa.Desactivar("Sistema");

            return await _empresaRepository.UpdateAsync(empresa) != null;
        }
    }
}
