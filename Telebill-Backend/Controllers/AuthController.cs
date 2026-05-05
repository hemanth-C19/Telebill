using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.Auth;
using Telebill.Services.Auth;

namespace Telebill.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]-Module")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var result = await authService.LoginAsync(loginDto);
        if (result == null)
            return Unauthorized();

        return Ok(result);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        authService.Logout();
        return Ok(new { message = "Logout successful" });
    }
}
