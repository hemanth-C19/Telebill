using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.PreCert;
using Telebill.Services.PreCert;

namespace Telebill.Controllers.PreCert;

[ApiController]
[Route("api/v1/precert/attachments")]
public class AttachmentController(IPreCertService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAttachmentRequestDto dto)
    {
        try
        {
            var result = await service.CreateAttachmentAsync(dto, GetCurrentUserId());
            return StatusCode(201, result);
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

    [HttpGet("by-claim/{claimID:int}")]
    public async Task<IActionResult> GetByClaim(int claimID, [FromQuery] string status = "Active")
    {
        try
        {
            var result = await service.GetAttachmentsByClaimAsync(claimID, status);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("{attachId:int}")]
    public async Task<IActionResult> GetById(int attachId)
    {
        try
        {
            var result = await service.GetAttachmentByIdAsync(attachId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPatch("{attachId:int}/status")]
    public async Task<IActionResult> UpdateStatus(int attachId, [FromBody] UpdateAttachmentStatusRequestDto dto)
    {
        try
        {
            var result = await service.UpdateAttachmentStatusAsync(attachId, dto, GetCurrentUserId());
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

    private int GetCurrentUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(id, out var parsed) ? parsed : 0;
    }
}

