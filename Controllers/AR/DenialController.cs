using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.AR;
using Telebill.Services.AR;

namespace Telebill.Controllers.AR;

[ApiController]
[Route("api/v1/ar/denials")]
public class DenialController(IDenialService denialService) : ControllerBase
{
    [HttpPatch("{denialId:int}/status")]
    public async Task<IActionResult> UpdateDenialStatus(
        int denialId, [FromBody] UpdateDenialStatusDto dto)
    {
        var (success, error) = await denialService.UpdateDenialStatusAsync(denialId, dto);
        if (!success)
        {
            return BadRequest(error);
        }

        return Ok();
    }

    [HttpPost("{denialId:int}/attachments")]
    public async Task<IActionResult> UploadAppealDocument(
        int denialId, [FromBody] UploadAppealDocumentDto dto)
    {
        dto.DenialId = denialId;
        var (success, error, result) = await denialService.UploadAppealDocumentAsync(dto);
        if (!success)
        {
            return BadRequest(error);
        }

        return CreatedAtAction(nameof(UploadAppealDocument), new { denialId }, result);
    }

    [HttpPost("reset-for-resubmission")]
    public async Task<IActionResult> ResetForResubmission([FromBody] ResetClaimForResubmissionDto dto)
    {
        var (success, error, result) = await denialService.ResetClaimForResubmissionAsync(dto);
        if (!success)
        {
            if (error == "Denial not found" || error == "Parent claim not found")
            {
                return NotFound(error);
            }

            return BadRequest(error);
        }

        return Ok(result);
    }
}

