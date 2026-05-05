using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace Telebill.Controllers;

[ApiController]
[Authorize(Roles = "FrontDesk,Coder,AR,Admin")]
public class X12Controller(IClaimService claimService) : ControllerBase
{
    [HttpPost("api/claims/{claimID:int}/generate-837p")]
    [Authorize(Roles = "FrontDesk,AR,Admin")]
    public async Task<IActionResult> Generate(int claimID)
    {
        try
        {
            var result = await claimService.Generate837PAsync(claimID);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("api/claims/{claimID:int}/837p")]
    public async Task<IActionResult> GetRef(int claimID)
    {
        var result = await claimService.Get837PRefAsync(claimID);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}

