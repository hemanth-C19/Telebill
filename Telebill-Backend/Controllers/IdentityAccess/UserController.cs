using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.IdentityAccess;
using Telebill.Models;
using Telebill.Services.IdentityAccess;

namespace Telebill.Controllers
{
    [ApiController]
    [Route("api/v1/IdentityAccess/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UserController(IUserService userService) : ControllerBase
    {
        [HttpPost]
        [Route("AddUser")]
        public async Task<IActionResult> AddUser(UserAddDTO user)
        {
            if (user is null)
                return BadRequest("Request body is required.");

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            await userService.AddUserAsync(user);
            return StatusCode(201, "user added");
        }

        [HttpPut]
        [Route("UpdateUser")]
        public async Task<IActionResult> UpdateUser(UserUpdateDTO userDTO)
        {
            if (userDTO is null)
                return BadRequest("Request body is required.");

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            await userService.UpdateAsync(userDTO);
            return Ok("record updated");
        }

        [HttpDelete]
        [Route("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] int id)
        {
            await userService.DeleteAsync(id);
            return Ok("deleted record");
        }

        [HttpGet]
        [Route("GetUsers")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? search,
            [FromQuery] string? role,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10
        )
        {
            var users = await userService.GetAllAsync(search, role, page, limit);
            return Ok(users);
        }
    }
}