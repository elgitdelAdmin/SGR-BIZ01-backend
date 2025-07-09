using AutoMapper;
using BCrypt.Net;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Constants;
using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;

namespace ConectaBiz.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IGestorRepository _gestorRepository;
        private readonly IConsultorRepository _consultorRepository;
        private readonly IPersonaService _personaService;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AuthService(
            IUserRepository userRepository, 
            IGestorRepository gestorRepository,
            IConsultorRepository consultorRepository,
            ITokenService tokenService, 
            IMapper mapper, 
            IPersonaService personaService)
        {
            _userRepository = userRepository;
            _gestorRepository = gestorRepository;
            _consultorRepository = consultorRepository;
            _tokenService = tokenService;
            _mapper = mapper;
            _personaService = personaService;
        }
        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }
        public async Task<IEnumerable<UserDto>> GetAllUsuarioByIdSocio(int idSocio)
        {
            var users = await _userRepository.GetAllUsuarioByIdSocio(idSocio);
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }
        public async Task<UserDto> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return _mapper.Map<UserDto>(user);
        }
        public async Task<UserDto> GetByIdSocioIdRolIdAsync(int idsocio, int idrol, int idpersona)
        {
            var user = await _userRepository.GetByIdSocioIdRolIdPersonaAsync(idsocio, idrol, idpersona);
            return _mapper.Map<UserDto>(user);
        }
        public async Task<IEnumerable<RolDto>> GetAllRolAsync()
        {
            var roles = await _userRepository.GetAllRolAsync();
            var rolesDto = _mapper.Map<IEnumerable<RolDto>>(roles);
            return rolesDto.Where(r => r.Codigo != AppConstants.Roles.SuperAdmin);
        }
        public async Task<RolDto> GetRolByIdAsync(int id)
        {
            var rol = await _userRepository.GetRolByIdAsync(id);
            return _mapper.Map<RolDto>(rol);
        }
        public async Task<RolDto> GetRolByCodigoAsync(string codigo)
        {
            var rol = await _userRepository.GetRolByCodigoAsync(codigo);
            return _mapper.Map<RolDto>(rol);
        }
        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginRequest)
        {
            var user = await _userRepository.GetByUsernameAsync(loginRequest.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Credenciales inválidas");
            }

            user.LastLogin = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            await _userRepository.UpdateAsync(user);

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                ExpiryDate = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(7), DateTimeKind.Unspecified),
                UserId = user.Id
            };

            await _userRepository.AddRefreshTokenAsync(refreshTokenEntity);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = _mapper.Map<UserDto>(user)
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterUserDto registerRequest)
        {
            var existingUser = await _userRepository.GetByUsernameAsync(registerRequest.Username);
            if (existingUser != null)
            {
                throw new InvalidOperationException("El nombre de usuario ya está en uso");
            }
            PersonaDto persona = await _personaService.ValidateCreateUpdate(registerRequest.Persona);
            if (persona == null || persona.Id == 0)
            {
                throw new InvalidOperationException("No se pudo validar o crear la persona.");
            }
            var user = new User
            {
                Username = registerRequest.Username,
                Email = registerRequest.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                IdSocio = registerRequest.IdSocio,
                IdRol = registerRequest.IdRol,
                IdPersona = persona.Id,
                Activo = true
            };

            var userCreado = await _userRepository.CreateAsync(user);
            var rol = await _userRepository.GetRolByIdAsync(registerRequest.IdRol);

            if (rol.Codigo == AppConstants.Roles.Gestor)
            {
                var gestor = new Gestor
                {
                    PersonaId = persona.Id,
                    IdNivelExperiencia = null,
                    IdModalidadLaboral = null,
                    IdSocio = registerRequest.IdSocio,
                    IdUser = userCreado.Id,
                    UsuarioCreacion = registerRequest.UsuarioCreacion,
                    FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
                    Activo = true
                };
                await _gestorRepository.CreateAsync(gestor);
            }
            if (rol.Codigo == AppConstants.Roles.Consultor)
            {
                var consultor = new Consultor
                {
                    PersonaId = persona.Id,
                    IdNivelExperiencia = null,
                    IdModalidadLaboral = null,
                    IdSocio = registerRequest.IdSocio,
                    IdUser = userCreado.Id,
                    UsuarioCreacion = registerRequest.UsuarioCreacion,
                    FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
                    Activo = true
                };
                await _consultorRepository.CreateAsync(consultor);
            }
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                ExpiryDate = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(7), DateTimeKind.Unspecified),
                UserId = user.Id
            };

            await _userRepository.AddRefreshTokenAsync(refreshTokenEntity);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = _mapper.Map<UserDto>(user)
            };
        }
        public async Task<UserDto?> UpdateUserAsync(UpdateUserDto updateUserDto)
        {
            var existingUser = await _userRepository.GetByIdAsync(updateUserDto.Id);
            if (existingUser == null)
            {
                return null;
            }

            // Verificar si el username ya existe para otro usuario
            if (existingUser.Username != updateUserDto.Username)
            {
                var userWithSameUsername = await _userRepository.GetByUsernameAsync(updateUserDto.Username);
                if (userWithSameUsername != null && userWithSameUsername.Id != updateUserDto.Id)
                {
                    throw new InvalidOperationException("El nombre de usuario ya está en uso");
                }
            }

            // Actualizar datos del usuario
            //existingUser.Username = updateUserDto.Username;
            //existingUser.Email = updateUserDto.Email;
            //existingUser.IdSocio = updateUserDto.IdSocio;
            //existingUser.IdRol = updateUserDto.IdRol;

            // Actualizar contraseña si se proporciona
            if (!string.IsNullOrEmpty(updateUserDto.Password))
            {
                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateUserDto.Password);
            }

            // Actualizar datos de la persona si se incluyen
            if (updateUserDto.Persona != null && existingUser.Persona != null)
            {
                var personaDto = new UpdatePersonaDto
                {
                    Id = existingUser.IdPersona,
                    Nombres = updateUserDto.Persona.Nombres,
                    ApellidoPaterno = updateUserDto.Persona.ApellidoPaterno,
                    ApellidoMaterno = updateUserDto.Persona.ApellidoMaterno,
                    NumeroDocumento = updateUserDto.Persona.NumeroDocumento,
                    TipoDocumento = updateUserDto.Persona.TipoDocumento,
                    Telefono = updateUserDto.Persona.Telefono,
                    Telefono2 = updateUserDto.Persona.Telefono2,
                    Correo = updateUserDto.Persona.Correo,
                    Direccion = updateUserDto.Persona.Direccion,
                    FechaNacimiento = updateUserDto.Persona.FechaNacimiento.HasValue
                        ? DateTime.SpecifyKind((DateTime)updateUserDto.Persona.FechaNacimiento, DateTimeKind.Local)
                        : null,
                    UsuarioActualizacion = updateUserDto.UsuarioActualizacion
                };
                await _personaService.ValidateUpdateAsync(personaDto);
            }

            var updatedUser = await _userRepository.UpdateUserAsync(existingUser);
            return _mapper.Map<UserDto>(updatedUser);
        }
        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _userRepository.DeleteUserAsync(id);
        }
        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _userRepository.GetRefreshTokenAsync(refreshToken);

            if (storedToken == null || storedToken.ExpiryDate < DateTime.UtcNow || storedToken.IsRevoked)
            {
                throw new UnauthorizedAccessException("Token de refresco inválido o expirado");
            }

            var user = storedToken.User;
            var accessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Revocar el token anterior
            await _userRepository.RevokeRefreshTokenAsync(refreshToken);

            // Crear nuevo token de refresco
            var refreshTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                UserId = user.Id
            };

            await _userRepository.AddRefreshTokenAsync(refreshTokenEntity);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = _mapper.Map<UserDto>(user)
            };
        }

        public async Task LogoutAsync(string refreshToken)
        {
            await _userRepository.RevokeRefreshTokenAsync(refreshToken);
        }
    }
}