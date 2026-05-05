using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.AR;
using Telebill.Services.AR;

namespace Telebill.Controllers.AR;

[ApiController]
[Route("api/v1/ar/underpayments")]
[Authorize(Roles = "AR,Admin")]
public class UnderpaymentController(IUnderpaymentService underpaymentService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUnderpayments()
    {
        var items = await underpaymentService.GetUnderpaymentWorklistAsync();
        return Ok(items);
    }

    [HttpPost("flag")]
    public async Task<IActionResult> FlagUnderpayment(
        [FromBody] FlagUnderpaymentDto dto,
        [FromQuery] int userId)
    {
        var (success, error) = await underpaymentService.FlagUnderpaymentAsync(dto, userId);
        if (!success)
        {
            if (error == "Claim not found")
            {
                return NotFound(error);
            }

            return BadRequest(error);
        }

        return Ok();
    }
}

