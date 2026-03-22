using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.PreCert;
using Telebill.Services.PreCert;

namespace Telebill.Controllers.PreCert;

[ApiController]
[Route("api/v1/precert/prior-auth")]
public class PriorAuthController(IPreCertService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePriorAuthRequestDto dto)
    {
        try
        {
            var result = await service.CreatePriorAuthAsync(dto, GetCurrentUserId());
            return StatusCode(201, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? claimID, [FromQuery] int? planID, [FromQuery] string? status, [FromQuery] bool? expiringSoon)
    {
        var result = await service.GetPriorAuthsAsync(claimID, planID, status, expiringSoon);
        return Ok(result);
    }

    [HttpGet("{paid:int}")]
    public async Task<IActionResult> GetById(int paid)
    {
        try
        {
            var result = await service.GetPriorAuthByIdAsync(paid);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("by-claim/{claimID:int}")]
    public async Task<IActionResult> GetByClaim(int claimID)
    {
        try
        {
            var result = await service.GetPriorAuthsByClaimAsync(claimID);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPatch("{paid:int}")]
    public async Task<IActionResult> Update(int paid, [FromBody] UpdatePriorAuthRequestDto dto)
    {
        try
        {
            var result = await service.UpdatePriorAuthAsync(paid, dto, GetCurrentUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{paid:int}")]
    public async Task<IActionResult> SoftDelete(int paid)
    {
        try
        {
            await service.SoftDeletePriorAuthAsync(paid, GetCurrentUserId());
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    private int GetCurrentUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(id, out var parsed) ? parsed : 0;
    }
}

