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
        private readonly Lazy<INotificacionTicketService> _notificacionTicketService;
        private readonly Lazy<ITicketService> _ticketService;
        private readonly IEmailService _emailService;

        public AuthService(
            IUserRepository userRepository, 
            IGestorRepository gestorRepository,
            IConsultorRepository consultorRepository,
            ITokenService tokenService, 
            IMapper mapper, 
            IPersonaService personaService,
            Lazy<INotificacionTicketService> notificacionTicketService,
            Lazy<ITicketService> ticketService,
            IEmailService emailService
        )
        {
            _userRepository = userRepository;
            _gestorRepository = gestorRepository;
            _consultorRepository = consultorRepository;
            _tokenService = tokenService;
            _mapper = mapper;
            _personaService = personaService;
            _notificacionTicketService = notificacionTicketService;
            _ticketService = ticketService;
            _notificacionTicketService = notificacionTicketService;
            _ticketService = ticketService;
            _emailService = emailService;
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
        public async Task<IEnumerable<UserDto>> GetUsersByIdAsync(int[] ids)
        {
            var users = await _userRepository.GetUsersByIdAsync(ids);
            return _mapper.Map<IEnumerable<UserDto>>(users);
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
            try
            {
                var user = await _userRepository.GetByUsernameAsync(loginRequest.Username);
                //await _ticketService.Value.ActualizarEstadoDeAprobadoAEnEjecucion();

                var notificacionTicketDto = await _notificacionTicketService.Value.GetNotificacionesByUserIdAsync(user.Id);

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

                var consultor = await _consultorRepository.GetByIdUserAsync(user.Id);
                return new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    User = _mapper.Map<UserDto>(user),
                    IdConsultor = consultor?.Id,
                    NotificacionTicket = notificacionTicketDto.ToList()
                };
            }
            catch (Exception ex)
            {

                throw;
            }

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

            if (rol.Codigo == AppConstants.Roles.GestorCuenta || rol.Codigo == AppConstants.Roles.GestorConsultoria)
            {
                if (!await _gestorRepository.ExistsByPersonaIdAsync(persona.Id))
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
                else {
                    var gestorExistente = await _gestorRepository.GetByIdPersonaAsync(persona.Id);
                    gestorExistente.IdUser = userCreado.Id;
                    gestorExistente.UsuarioActualizacion = registerRequest.UsuarioCreacion;
                    gestorExistente.FechaActualizacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                    await _gestorRepository.UpdateAsync(gestorExistente);
                }
            }
            if (rol.Codigo == AppConstants.Roles.Consultor)
            {
                if (!await _consultorRepository.ExistsByPersonaIdAsync(persona.Id))
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
                else
                {
                    var consultorexistente = await _consultorRepository.GetByIdPersonaAsync(persona.Id);
                    consultorexistente.IdUser = userCreado.Id;
                    consultorexistente.UsuarioActualizacion = registerRequest.UsuarioCreacion;
                    consultorexistente.FechaActualizacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                    await _consultorRepository.UpdateUserAsync(consultorexistente);
                }
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
            existingUser.Email = updateUserDto.Email;
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
        // En ConectaBiz.Application.Services.AuthService (agregar al final de la clase)

        public async Task<OperationResultDto> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            try
            {
                // Obtener usuario
                var user = await _userRepository.GetByIdAsync(changePasswordDto.UserId);
                if (user == null)
                {
                    return new OperationResultDto
                    {
                        Success = false,
                        Message = "Usuario no encontrado"
                    };
                }

                // Validar contraseña actual
                if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
                {
                    return new OperationResultDto
                    {
                        Success = false,
                        Message = "La contraseña actual es incorrecta"
                    };
                }

                // Validar que la nueva contraseña sea diferente
                if (BCrypt.Net.BCrypt.Verify(changePasswordDto.NewPassword, user.PasswordHash))
                {
                    return new OperationResultDto
                    {
                        Success = false,
                        Message = "La nueva contraseña debe ser diferente a la actual"
                    };
                }

                // Actualizar contraseña
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
                await _userRepository.UpdateAsync(user);

                return new OperationResultDto
                {
                    Success = true,
                    Message = "Contraseña actualizada exitosamente"
                };
            }
            catch (Exception ex)
            {
                return new OperationResultDto
                {
                    Success = false,
                    Message = $"Error al cambiar la contraseña: {ex.Message}"
                };
            }
        }

        public async Task<OperationResultDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                // Buscar usuario por email
                var user = await _userRepository.GetByEmailAsync(forgotPasswordDto.Email);
                if (user == null)
                {
                    // Por seguridad, no revelar si el email existe
                    return new OperationResultDto
                    {
                        Success = true,
                        Message = "Si el correo existe, recibirás un código para restablecer tu contraseña"
                    };
                }

                // Generar token más corto y amigable (6 números aleatorios)
                var random = new Random();
                var token = random.Next(100000, 999999).ToString();

                // Crear registro de token
                var resetToken = new PasswordResetToken
                {
                    Token = token,
                    UserId = user.Id,
                    ExpiryDate = DateTime.SpecifyKind(DateTime.UtcNow.AddHours(1), DateTimeKind.Unspecified),
                    IsUsed = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                };

                await _userRepository.AddPasswordResetTokenAsync(resetToken);

                // Enviar correo con el código/token visible
                var mensaje = $@"
                                <html>
                                <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                                    <div style='max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; padding: 30px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                                        <h2 style='color: #333; text-align: center;'>Recuperación de Contraseña</h2>
                    
                                        <p style='color: #666; font-size: 16px;'>Hola <strong>{user.Username}</strong>,</p>
                    
                                        <p style='color: #666; font-size: 16px;'>
                                            Has solicitado restablecer tu contraseña. Utiliza el siguiente código:
                                        </p>
                    
                                        <div style='background-color: #f0f4f8; border: 2px dashed #4a90e2; border-radius: 8px; padding: 20px; margin: 30px 0; text-align: center;'>
                                            <p style='color: #666; margin: 0 0 10px 0; font-size: 14px;'>Tu código de recuperación es:</p>
                                            <div style='background-color: #4a90e2; color: white; font-size: 32px; font-weight: bold; letter-spacing: 8px; padding: 20px; border-radius: 5px; font-family: monospace;'>
                                                {token}
                                            </div>
                                        </div>
                    
                                        <p style='color: #666; font-size: 14px; text-align: center;'>
                                            <strong>⏰ Este código expirará en 1 hora</strong>
                                        </p>
                    
                                        <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
                    
                                        <p style='color: #999; font-size: 12px; text-align: center;'>
                                            Si no solicitaste este cambio, ignora este correo.<br>
                                            Tu contraseña no será modificada hasta que ingreses el código.
                                        </p>
                                    </div>
                                </body>
                                </html>";

                await _emailService.EnviarCorreosAsync(
                    new[] { user.Persona.Correo },
                    "Recuperación de Contraseña - Código de Seguridad",
                    mensaje
                );

                return new OperationResultDto
                {
                    Success = true,
                    Message = "Si el correo existe, recibirás un código para restablecer tu contraseña"
                };
            }
            catch (Exception ex)
            {
                return new OperationResultDto
                {
                    Success = false,
                    Message = $"Error al procesar la solicitud: {ex.Message}"
                };
            }
        }

        public async Task<OperationResultDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                // Validar token
                var resetToken = await _userRepository.GetPasswordResetTokenAsync(resetPasswordDto.Token);
                if (resetToken == null || resetToken.IsUsed)
                {
                    return new OperationResultDto
                    {
                        Success = false,
                        Message = "Token inválido o ya utilizado"
                    };
                }

                if (resetToken.ExpiryDate < DateTime.UtcNow)
                {
                    return new OperationResultDto
                    {
                        Success = false,
                        Message = "El token ha expirado"
                    };
                }

                // Actualizar contraseña
                var user = resetToken.User;
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);
                await _userRepository.UpdateAsync(user);

                // Marcar token como usado
                await _userRepository.MarkPasswordResetTokenAsUsedAsync(resetPasswordDto.Token);

                // Enviar correo de confirmación
                var mensaje = $@"
            <h2>Contraseña Actualizada</h2>
            <p>Hola {user.Username},</p>
            <p>Tu contraseña ha sido actualizada exitosamente.</p>
            <p>Si no realizaste este cambio, contacta inmediatamente con soporte.</p>";

                await _emailService.EnviarCorreosAsync(
                    new[] { resetToken.User.Persona.Correo},
                    "Contraseña Actualizada",
                    mensaje
                );

                return new OperationResultDto
                {
                    Success = true,
                    Message = "Contraseña restablecida exitosamente"
                };
            }
            catch (Exception ex)
            {
                return new OperationResultDto
                {
                    Success = false,
                    Message = $"Error al restablecer la contraseña: {ex.Message}"
                };
            }
        }

        public async Task<OperationResultDto> SendEmailVerificationCodeAsync(VerifyEmailDto verifyEmailDto)
        {
            try
            {
                // Invalidar códigos anteriores no usados
                await _userRepository.InvalidateOldVerificationCodesAsync(verifyEmailDto.Email);

                // Generar código de 6 dígitos
                var random = new Random();
                var code = random.Next(100000, 999999).ToString();

                // Crear registro de verificación
                var verificationCode = new EmailVerificationCode
                {
                    Email = verifyEmailDto.Email,
                    Code = code,
                    ExpiryDate = DateTime.SpecifyKind(DateTime.UtcNow.AddMinutes(15), DateTimeKind.Unspecified),
                    IsUsed = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                };

                await _userRepository.AddEmailVerificationCodeAsync(verificationCode);

                var codeSpaced = string.Join("&nbsp;", code.ToCharArray());

                var mensaje = $@"
<table role='presentation' width='100%' cellpadding='0' cellspacing='0' border='0' style='background:#f4f4f4;'>
  <tr>
    <td align='center' style='padding:20px; color:#333333; font-family:Arial, sans-serif;'>

      <table role='presentation' width='420' cellpadding='0' cellspacing='0' border='0' style='background:#ffffff; border-radius:6px;'>
        <tr>
          <td align='center' style='padding:24px; color:#333333; font-family:Arial, sans-serif;'>

            <div style='font-size:22px; line-height:28px; font-weight:bold; color:#333333; margin:0 0 10px 0;'>
              Código de Verificación
            </div>

            <div style='font-size:14px; line-height:20px; color:#555555; margin:0 0 14px 0;'>
              Tu código de verificación es:
            </div>

            <table role='presentation' cellpadding='0' cellspacing='0' border='0' align='center'>
              <tr>
                <td align='center' bgcolor='#4a90e2'
                    style='background:#4a90e2; padding:14px 18px; font-family:Arial, sans-serif; font-size:36px; line-height:42px; font-weight:bold; color:#ffffff; mso-line-height-rule:exactly; mso-padding-alt:14px 18px 14px 18px;'>
                  {codeSpaced}
                </td>
              </tr>
            </table>

            <div style='font-size:14px; line-height:20px; color:#555555; margin:16px 0 0 0;'>
              Este código expirará en 15 minutos.
            </div>

            <div style='font-size:12px; line-height:18px; color:#777777; margin:10px 0 0 0;'>
              Si no solicitaste este código, ignora este correo.
            </div>

          </td>
        </tr>
      </table>

    </td>
  </tr>
</table>";


                await _emailService.EnviarCorreosAsync(
                    new[] { verifyEmailDto.Email },
                    "Código de Verificación",
                    mensaje
                );

                return new OperationResultDto
                {
                    Success = true,
                    Message = "Código de verificación enviado al correo electrónico"
                };
            }
            catch (Exception ex)
            {
                return new OperationResultDto
                {
                    Success = false,
                    Message = $"Error al enviar código de verificación: {ex.Message}"
                };
            }
        }

        public async Task<OperationResultDto> ConfirmEmailVerificationAsync(ConfirmEmailDto confirmEmailDto)
        {
            try
            {
                // Validar código
                var verificationCode = await _userRepository.GetEmailVerificationCodeAsync(
                    confirmEmailDto.Email,
                    confirmEmailDto.Code
                );

                if (verificationCode == null || verificationCode.IsUsed)
                {
                    return new OperationResultDto
                    {
                        Success = false,
                        Message = "Código inválido o ya utilizado"
                    };
                }

                if (verificationCode.ExpiryDate < DateTime.UtcNow)
                {
                    return new OperationResultDto
                    {
                        Success = false,
                        Message = "El código ha expirado"
                    };
                }

                // Marcar código como usado
                await _userRepository.MarkEmailVerificationCodeAsUsedAsync(verificationCode.Id);

                return new OperationResultDto
                {
                    Success = true,
                    Message = "Correo electrónico verificado exitosamente"
                };
            }
            catch (Exception ex)
            {
                return new OperationResultDto
                {
                    Success = false,
                    Message = $"Error al verificar el código: {ex.Message}"
                };
            }
        }
    }
}