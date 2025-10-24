using AutoMapper;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Application.Services;
using ConectaBiz.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
        {
            try
            {
                var usersDto = await _authService.GetAllAsync();
                return Ok(usersDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los consultores");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al procesar la solicitud");
            }
        }
        [HttpGet("usuario/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetById(int id)
        {
            try
            {
                var userDto = await _authService.GetByIdAsync(id);

                if (userDto == null)
                {
                    return NotFound("Usuario no encontrado");
                }
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al procesar la solicitud");
            }
        }

        [HttpGet("usuarioByIdSocio/{idSocio}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsuarioByIdSocio(int idSocio)
        {
            try
            {
                var usersDto = await _authService.GetAllUsuarioByIdSocio(idSocio);
                return Ok(usersDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los consultores");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al procesar la solicitud");
            }
        }

        [HttpGet("roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<RolDto>>> GetAllRol()
        {
            try
            {
                var usersDto = await _authService.GetAllRolAsync();
                return Ok(usersDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los consultores");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al procesar la solicitud");
            }
        }
        [HttpPut("UpdateUser/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            try
            {
                if (id != updateUserDto.Id)
                {
                    return BadRequest("El ID del usuario no coincide");
                }

                var userDto = await _authService.UpdateUserAsync(updateUserDto);
                if (userDto == null)
                {
                    return NotFound("Usuario no encontrado");
                }

                return Ok(userDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al procesar la solicitud");
            }
        }
        [HttpDelete("DeleteUser/{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _authService.DeleteUserAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"No se encontró el Usario con ID {id}" });
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterUserDto registerDto)
        {
            var response = await _authService.RegisterAsync(registerDto);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto loginDto)
        {
            try
            {
                var response = await _authService.LoginAsync(loginDto);
                return Ok(response);
            }
            catch (Exception ex)
            {

                throw;
            }
           
        }
        [HttpPost("MarcarNotificacionComoLeida")]
        public async Task<ActionResult<AuthResponseDto>> MarcarComoLeidaAsync(int idUser, int[] idsNotificaciones)
        {
            try
            {
                var response = await _notificacionTicketService.MarcarComoLeidaAsync(idUser, idsNotificaciones);
                return Ok(response);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto refreshTokenDto)
        {
            var response = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto refreshTokenDto)
        {
            await _authService.LogoutAsync(refreshTokenDto.RefreshToken);
            return Ok(new { message = "Sesión cerrada correctamente" });
        }
    }
}