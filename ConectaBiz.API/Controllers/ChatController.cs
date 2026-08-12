using ConectaBiz.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ConectaBiz.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatAgent _chatAgent;

        public ChatController(IChatAgent chatAgent)
        {
            _chatAgent = chatAgent;
        }

        // [Authorize] // Puedes descomentarlo luego
        // [Authorize] 
        [HttpPost("preguntar")]
        public async Task<IActionResult> Preguntar([FromBody] string mensaje)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
                return BadRequest("El mensaje no puede estar vacío.");

            var resultado = await _chatAgent.PreguntarAsync(mensaje);

            return Ok(resultado);
        }

    }
}
