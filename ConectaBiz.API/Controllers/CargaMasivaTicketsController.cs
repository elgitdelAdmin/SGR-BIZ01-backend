using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConectaBiz.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CargaMasivaTicketsController : ControllerBase
    {
        private readonly ICargaMasivaTicketsService _cargaMasivaTickets;

        public CargaMasivaTicketsController(ICargaMasivaTicketsService cargaMasivaTickets)
        {
            _cargaMasivaTickets = cargaMasivaTickets;
        }

        [HttpPost("CargaMasiva")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CargaMasiva([FromForm] string tipoCarga, [FromForm(Name = "excel")] IFormFile excel)
        {
            if (excel == null || excel.Length == 0)
                return BadRequest("Debe subir un archivo Excel válido.");

            using var stream = excel.OpenReadStream();

            var datos = await _cargaMasivaTickets.ProcesarExcelAsync(stream, tipoCarga);

            return Ok(new
            {
                Mensaje = "Archivo procesado correctamente",
                Tipo = tipoCarga,
                NombreArchivo = excel.FileName,
                Peso = excel.Length,
                FilasLeidas = datos.Count,
                Datos = datos
            });
        }
    }
}
