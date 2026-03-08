using System;
using Microsoft.AspNetCore.Mvc;
using Telebill.Services.Auth;

namespace Telebill.Controllers
{
    [ApiController]
    [Route("api/[controller]-Module")]
    public class AuthController : ControllerBase
    {
        IAuthService authService;

        public AuthController(IAuthService _authService)
        {
            this.authService = _authService;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(string email)
        {
            bool loginSuccess = await authService.Login(email);
            if (loginSuccess)
                return Ok(new { message = "Login successful" });
            else
                return NotFound(new { message = "Account does not exist" });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            authService.Logout();
            return Ok(new { message = "Logout successful" });
        }
    }
}