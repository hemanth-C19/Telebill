using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.Posting;
using Telebill.Services.Posting;

namespace Telebill.Controllers.Posting;

[ApiController]
[Route("api/v1/posting/remits")]
public class RemitController : ControllerBase
{
    private readonly IPostingService _service;

    public RemitController(IPostingService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRemitRefRequestDto dto)
    {
        try
        {
            var result = await _service.CreateRemitRefAsync(dto, GetCurrentUserId());
            return StatusCode(201, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? payerID,
        [FromQuery] string? status,
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var result = await _service.GetRemitRefsAsync(payerID, status, dateFrom, dateTo, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{remitID:int}")]
    public async Task<IActionResult> GetById(int remitID)
    {
        try
        {
            var result = await _service.GetRemitRefByIdAsync(remitID);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPatch("{remitID:int}/status")]
    public async Task<IActionResult> UpdateStatus(int remitID, [FromBody] UpdateRemitRefStatusRequestDto dto)
    {
        try
        {
            var result = await _service.UpdateRemitRefStatusAsync(remitID, dto, GetCurrentUserId());
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
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

