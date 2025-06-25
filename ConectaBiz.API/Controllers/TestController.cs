using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConectaBiz.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        //[HttpGet("public")]
        //public IActionResult PublicEndpoint()
        //{
        //    return Ok(new { message = "Este es un endpoint público" });
        //}

        //[Authorize]
        //[HttpGet("protected")]
        //public IActionResult ProtectedEndpoint()
        //{
        //    return Ok(new { message = "Este es un endpoint protegido. Solo usuarios autenticados pueden acceder." });
        //}
    }
}