using Blog.Attributes;
using Microsoft.AspNetCore.Mvc;

//Health Check
namespace Blog.Controllers;

[ApiController]
[Route("")]
public class HomeController : ControllerBase
{
    [HttpGet("")]
    // [ApiKey] Pode usar como autenticação ou ApiKey ou JWT Token
    public IActionResult Get()
    {
        return Ok();
    }
}