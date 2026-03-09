using System;
using Microsoft.AspNetCore.Mvc;
using Telebill.Services.Auth;
using Telebill.Data;



namespace Telebill.Controllers
{
    [ApiController]
    [Route("api/[controller]-Module")]
    public class AuthController : ControllerBase
    {
        IAuthService _authService = new AuthService();

        [HttpPost]
        [Route("login")]
        public IActionResult Login(string email)
        {
            bool loginSuccess = _authService.Login(email);
            if (loginSuccess)
                return Ok(new { message = "Login successful" });
            else
                return NotFound(new { message = "Account does not exist" });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            _authService.Logout();
            return Ok(new { message = "Logout successful" });
        }
    }
}