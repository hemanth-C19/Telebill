using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.IdentityAccess;
using Telebill.Models;
using Telebill.Services.IdentityAccess;

namespace Telebill.Controllers
{
    [ApiController]
    // Base URL: /MyProject/User
    [Route("api/v1/IdentityAccess/[controller]")]
    public class UserController(IUserService userService) : ControllerBase
    {
        // POST: /MyProject/User/AddUser
        [HttpPost]
        [Route("AddUser")]
        public async Task<IActionResult> AddUser(UserDTO user)
        {
            if (user is null)
                return BadRequest("Request body is required.");

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            await userService.AddAsync(user);
            // Keeping your original response style
            return StatusCode(201, "user added");
        }

        // PUT: /MyProject/User/UpdateUser?id=123
        // (kept signature style similar to your previous code)
        [HttpPut]
        [Route("UpdateUser")]
        public async Task<IActionResult> UpdateUser(UserDTO userDTO, int id)
        {
            if (userDTO is null)
                return BadRequest("Request body is required.");

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            await userService.UpdateAsync(userDTO, id);
            // Matching your previous message pattern
            return Ok("record updated");
        }

        // DELETE: /MyProject/User/DeleteUser?id=123
        [HttpDelete]
        [Route("DeleteUser")]
        public async Task<IActionResult> DeleteUser([FromQuery] int id)
        {
            await userService.DeleteAsync(id);
            return Ok("deleted record");
        }

        // GET: /MyProject/User/GetUsers
        [HttpGet]
        [Route("GetUsers")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await userService.GetAllAsync();
            return Ok(users);
        }
        [HttpGet]
        [Route("GetUserByrole")]
        public async Task<IActionResult> getUsersbyRole(string role)
        {
            var user=await userService.Getuserbyrole(role);
            return Ok(user);
        }
    }
}