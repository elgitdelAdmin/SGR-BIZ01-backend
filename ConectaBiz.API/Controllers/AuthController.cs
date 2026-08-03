using AutoMapper;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ConectaBiz.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly INotificacionTicketService _notificacionTicketService;        
        private readonly ILogger<AuthController> _logger;
        private readonly IMapper _mapper;

        public AuthController(IAuthService authService,
                        INotificacionTicketService notificacionTicketService,
                        ILogger<AuthController> logger,
                        IMapper mapper
        )
        {
            _authService = authService;
            _notificacionTicketService = notificacionTicketService;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("usuario")]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
        {
            var usersDto = await _authService.GetAllAsync();
            return Ok(usersDto);
        }

        [HttpGet("usuario/{id}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserDto>> GetById(int id)
        {
            var userDto = await _authService.GetByIdAsync(id);

            if (userDto == null)
            {
                throw new KeyNotFoundException("Usuario no encontrado");
            }
            return Ok(userDto);
        }

        [HttpGet("usuarioByIdSocio/{idSocio}")]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsuarioByIdSocio(int idSocio)
        {
            var usersDto = await _authService.GetAllUsuarioByIdSocio(idSocio);
            return Ok(usersDto);
        }

        [HttpGet("roles")]
        [ProducesResponseType(typeof(IEnumerable<RolDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<RolDto>>> GetAllRol()
        {
            bool isSuperAdmin = User.IsInRole("SUPERADMIN");
            var usersDto = await _authService.GetAllRolAsync(isSuperAdmin);
            return Ok(usersDto);
        }

        [HttpPut("UpdateUser/{id}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            if (id != updateUserDto.Id)
            {
                throw new InvalidOperationException("El ID del usuario no coincide");
            }

            var userDto = await _authService.UpdateUserAsync(updateUserDto);
            if (userDto == null)
            {
                throw new KeyNotFoundException("Usuario no encontrado");
            }

            return Ok(userDto);
        }

        [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
        [HttpDelete("DeleteUser/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var result = await _authService.DeleteUserAsync(id);
            if (!result)
            {
                throw new KeyNotFoundException($"No se encontró el Usario con ID {id}");
            }
            return NoContent();
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterUserDto registerDto)
        {
            var response = await _authService.RegisterAsync(registerDto);
            return Ok(response);
        }

        [AllowAnonymous]
        [EnableRateLimiting("auth-strict")]
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto loginDto)
        {
            var response = await _authService.LoginAsync(loginDto);
            return Ok(response);
        }

        [AllowAnonymous]
        [EnableRateLimiting("auth-strict")]
        [HttpPost("login-step2")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthResponseDto>> LoginStep2([FromBody] LoginStep2RequestDto requestDto)
        {
            var response = await _authService.LoginStep2Async(requestDto);
            return Ok(response);
        }

        [HttpPost("MarcarNotificacionComoLeida")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthResponseDto>> MarcarComoLeidaAsync([FromBody] MarcarNotificacionesLeidaDto dto)
        {
            var response = await _notificacionTicketService.MarcarComoLeidaAsync(dto.IdUser, dto.IdsNotificaciones);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto refreshTokenDto)
        {
            var response = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto refreshTokenDto)
        {
            await _authService.LogoutAsync(refreshTokenDto.RefreshToken);
            return Ok(new { message = "Sesión cerrada correctamente" });
        }

        [HttpPost("change-password")]
        [ProducesResponseType(typeof(OperationResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OperationResultDto>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var result = await _authService.ChangePasswordAsync(changePasswordDto);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [AllowAnonymous]
        [EnableRateLimiting("auth-strict")]
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(OperationResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OperationResultDto>> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            var result = await _authService.ForgotPasswordAsync(forgotPasswordDto);
            return Ok(result);
        }

        [AllowAnonymous]
        [EnableRateLimiting("auth-strict")]
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(OperationResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OperationResultDto>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            var result = await _authService.ResetPasswordAsync(resetPasswordDto);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [AllowAnonymous]
        [EnableRateLimiting("auth-strict")]
        [HttpPost("send-verification-code")]
        [ProducesResponseType(typeof(OperationResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OperationResultDto>> SendVerificationCode([FromBody] VerifyEmailDto verifyEmailDto)
        {
            var result = await _authService.SendEmailVerificationCodeAsync(verifyEmailDto);
            return Ok(result);
        }

        [AllowAnonymous]
        [EnableRateLimiting("auth-strict")]
        [HttpPost("confirm-email")]
        [ProducesResponseType(typeof(OperationResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OperationResultDto>> ConfirmEmail([FromBody] ConfirmEmailDto confirmEmailDto)
        {
            var result = await _authService.ConfirmEmailVerificationAsync(confirmEmailDto);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}