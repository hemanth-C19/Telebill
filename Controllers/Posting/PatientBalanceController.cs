using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.Posting;
using Telebill.Services.Posting;

namespace Telebill.Controllers.Posting;

[ApiController]
[Route("api/v1/posting/balances")]
public class PatientBalanceController(IPostingService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? patientID,
        [FromQuery] string? agingBucket,
        [FromQuery] string status = "Open",
        [FromQuery] decimal? minAmount = null,
        [FromQuery] decimal? maxAmount = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var filters = new PatientBalanceFilterParams
        {
            PatientID = patientID,
            AgingBucket = agingBucket,
            Status = status,
            MinAmount = minAmount,
            MaxAmount = maxAmount,
            Page = page,
            PageSize = pageSize
        };

        var result = await service.GetPatientBalancesAsync(filters);
        return Ok(result);
    }

    [HttpGet("aging-summary")]
    public async Task<IActionResult> GetAgingSummary()
    {
        var result = await service.GetAgingSummaryAsync();
        return Ok(result);
    }

    [HttpGet("{balanceID:int}")]
    public async Task<IActionResult> GetById(int balanceID)
    {
        try
        {
            var result = await service.GetPatientBalanceByIdAsync(balanceID);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("by-patient/{patientID:int}")]
    public async Task<IActionResult> GetByPatient(int patientID)
    {
        try
        {
            var result = await service.GetBalancesByPatientAsync(patientID);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPatch("{balanceID:int}/status")]
    public async Task<IActionResult> UpdateStatus(int balanceID, [FromBody] UpdatePatientBalanceStatusRequestDto dto)
    {
        try
        {
            var result = await service.UpdatePatientBalanceStatusAsync(balanceID, dto, GetCurrentUserId());
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

    [HttpPost("run-aging-job")]
    public async Task<IActionResult> RunAgingJob([FromQuery] string? schedulerKey)
    {
        try
        {
            var result = await service.RunAgingBucketJobAsync(schedulerKey);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    private int GetCurrentUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(id, out var parsed) ? parsed : 0;
    }
}

