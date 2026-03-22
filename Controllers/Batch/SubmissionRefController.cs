using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.Batch;
using Telebill.Services.Batch;

namespace Telebill.Controllers.Batch;

[ApiController]
[Route("api/v1/batch")]
public class SubmissionRefController(IBatchService service) : ControllerBase
{
    [HttpPost("{batchID:int}/ack/999")]
    public async Task<IActionResult> Record999(int batchID, [FromBody] Record999AckRequestDto dto)
    {
        try
        {
            var result = await service.Record999AckAsync(batchID, dto, GetCurrentUserId());
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

    [HttpPost("{batchID:int}/ack/277ca/{claimID:int}")]
    public async Task<IActionResult> Record277(int batchID, int claimID, [FromBody] Record277CAAckRequestDto dto)
    {
        try
        {
            var result = await service.Record277CAAckAsync(batchID, claimID, dto, GetCurrentUserId());
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

    [HttpGet("{batchID:int}/submission-refs")]
    public async Task<IActionResult> GetRefsForBatch(int batchID)
    {
        try
        {
            var result = await service.GetSubmissionRefsForBatchAsync(batchID);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("submission-refs/by-claim/{claimID:int}")]
    public async Task<IActionResult> GetRefsByClaim(int claimID)
    {
        try
        {
            var result = await service.GetSubmissionRefsByClaimAsync(claimID);
            return Ok(result);
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

