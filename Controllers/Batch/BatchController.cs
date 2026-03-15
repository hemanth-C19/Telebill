using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.Batch;
using Telebill.Services.Batch;

namespace Telebill.Controllers.Batch
{
    [ApiController]
    [Route("api/v1/batch")]
public class BatchController : ControllerBase
{
    private readonly IBatchService _service;

    public BatchController(IBatchService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBatchRequestDto dto)
    {
        var result = await _service.CreateBatchAsync(dto, GetCurrentUserId());
        return StatusCode(201, result);
    }

    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var result = await _service.GetBatchesAsync(status, dateFrom, dateTo, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{batchID:int}")]
    public async Task<IActionResult> GetDetail(int batchID)
    {
        try
        {
            var result = await _service.GetBatchDetailAsync(batchID);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("{batchID:int}/claims")]
    public async Task<IActionResult> AddClaims(int batchID, [FromBody] AddClaimsToBatchRequestDto dto)
    {
        try
        {
            var result = await _service.AddClaimsToBatchAsync(batchID, dto, GetCurrentUserId());
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
    public async Task<IActionResult> RemoveClaim(int batchID, int claimID)
    {
        try
        {
            await _service.RemoveClaimFromBatchAsync(batchID, claimID, GetCurrentUserId());
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
    public async Task<IActionResult> Generate(int batchID)
    {
        try
        {
            var result = await _service.GenerateBatchAsync(batchID, GetCurrentUserId());
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
    public async Task<IActionResult> Submit(int batchID, [FromBody] MarkSubmittedRequestDto dto)
    {
        try
        {
            var result = await _service.MarkBatchSubmittedAsync(batchID, dto, GetCurrentUserId());
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
}