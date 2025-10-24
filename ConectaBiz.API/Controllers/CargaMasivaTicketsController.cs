using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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
        public async Task<IActionResult> CargaMasiva([FromForm] CargaMasivaDto dto)
        {
            if (dto.Excel == null || dto.Excel.Length == 0)
                return BadRequest("Debe subir un archivo Excel válido.");


            using var stream = dto.Excel.OpenReadStream();

            // Llamar al servicio para procesar el Excel
            var datos = await _cargaMasivaTickets.ProcesarExcelAsync(stream, dto.TipoCarga);

            return Ok(new
            {
                Mensaje = "Archivo procesado correctamente",
                Tipo = dto.TipoCarga,
                NombreArchivo = dto.Excel.FileName,
                Peso = dto.Excel.Length,
                FilasLeidas = datos.Count,
                Datos = datos
            });
        }


    }
}
