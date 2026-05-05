using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.Batch;
using Telebill.Services.Batch;

namespace Telebill.Controllers.Batch;

[ApiController]
[Route("api/v1/batch")]
[Authorize(Roles = "FrontDesk,AR,Admin")]
public class BatchController(IBatchService service) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "FrontDesk,AR,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateBatchRequestDto dto)
    {
        var result = await service.CreateBatchAsync(dto, GetCurrentUserId());
        return StatusCode(201, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var result = await service.GetBatchesAsync(status, dateFrom, dateTo, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{batchID:int}")]
    public async Task<IActionResult> GetDetail(int batchID)
    {
        try
        {
            var result = await service.GetBatchDetailAsync(batchID);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("{batchID:int}/claims")]
    [Authorize(Roles = "FrontDesk,Admin")]
    public async Task<IActionResult> AddClaims(int batchID, [FromBody] AddClaimsToBatchRequestDto dto)
    {
        try
        {
            var result = await service.AddClaimsToBatchAsync(batchID, dto, GetCurrentUserId());
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

    [HttpDelete("{batchID:int}/claims/{claimID:int}")]
    [Authorize(Roles = "FrontDesk,Admin")]
    public async Task<IActionResult> RemoveClaim(int batchID, int claimID)
    {
        try
        {
            await service.RemoveClaimFromBatchAsync(batchID, claimID, GetCurrentUserId());
            return NoContent();
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

    [HttpPost("{batchID:int}/generate")]
    [Authorize(Roles = "FrontDesk,AR,Admin")]
    public async Task<IActionResult> Generate(int batchID)
    {
        try
        {
            var result = await service.GenerateBatchAsync(batchID, GetCurrentUserId());
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

    [HttpPost("{batchID:int}/submit")]
    [Authorize(Roles = "FrontDesk,AR,Admin")]
    public async Task<IActionResult> Submit(int batchID, [FromBody] MarkSubmittedRequestDto dto)
    {
        try
        {
            var result = await service.MarkBatchSubmittedAsync(batchID, dto, GetCurrentUserId());
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

