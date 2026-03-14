using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.Posting;
using Telebill.Services.Posting;

namespace Telebill.Controllers.Posting;

[ApiController]
[Route("api/v1/posting/statements")]
public class StatementController : ControllerBase
{
    private readonly IPostingService _service;

    public StatementController(IPostingService service)
    {
        _service = service;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateStatementRequestDto dto)
    {
        try
        {
            var result = await _service.GenerateStatementAsync(dto, GetCurrentUserId());
            return StatusCode(201, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("generate-batch")]
    public async Task<IActionResult> GenerateBatch([FromBody] GenerateStatementBatchRequestDto dto)
    {
        try
        {
            var result = await _service.GenerateStatementBatchAsync(dto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? patientID,
        [FromQuery] string? status,
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var result = await _service.GetStatementsAsync(patientID, status, dateFrom, dateTo, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{statementID:int}")]
    public async Task<IActionResult> GetById(int statementID)
    {
        try
        {
            var result = await _service.GetStatementByIdAsync(statementID);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPatch("{statementID:int}/status")]
    public async Task<IActionResult> UpdateStatus(int statementID, [FromBody] UpdateStatementStatusRequestDto dto)
    {
        try
        {
            var result = await _service.UpdateStatementStatusAsync(statementID, dto, GetCurrentUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private int GetCurrentUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(id, out var parsed) ? parsed : 0;
    }
}

