using System;
using Microsoft.AspNetCore.Mvc;

namespace Telebill.Controllers
{
    [ApiController]
    [Route("api/[controller]-Module")]
    public class EmployeeController : ControllerBase
    {
        [HttpGet]
        [Route("SayHi")]
        public IActionResult GetEmployees()
        {
            return Ok("Hi");
        }
    }
}