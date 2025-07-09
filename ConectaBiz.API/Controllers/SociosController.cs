using Microsoft.AspNetCore.Mvc;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class SociosController : ControllerBase
{
    private readonly ISocioService _socioService;

    public SociosController(ISocioService socioService)
    {
        _socioService = socioService;
    }

    [HttpGet]
    public async Task<ActionResult<List<SocioDto>>> ListarTodos()
    {
        var socios = await _socioService.ListarTodosAsync();
        return Ok(socios);
    }
}