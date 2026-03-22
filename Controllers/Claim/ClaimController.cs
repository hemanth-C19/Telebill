using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Services;
using Telebill.Dto;

namespace Telebill.Controllers;

[ApiController]
[Route("api/claims")]
public class ClaimController(IClaimService claimService) : ControllerBase
{
    [HttpPost("build")]
    public async Task<IActionResult> BuildClaim([FromBody] BuildClaimRequestDto dto)
    {
        try
        {
            var result = await claimService.BuildClaimAsync(dto);
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
    }

    [HttpGet]
    public async Task<IActionResult> GetClaims(
        [FromQuery] string? claimStatus,
        [FromQuery] int? patientID,
        [FromQuery] int? planID,
        [FromQuery] int? providerID,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] bool? hasScrubErrors,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string sortBy = "CreatedDate",
        [FromQuery] string sortOrder = "desc")
    {
        var filters = new ClaimFilterParams
        {
            ClaimStatus = claimStatus,
            PatientID = patientID,
            PlanID = planID,
            ProviderID = providerID,
            DateFrom = dateFrom,
            DateTo = dateTo,
            HasScrubErrors = hasScrubErrors,
            Page = page,
            PageSize = Math.Min(pageSize, 100),
            SortBy = sortBy,
            SortOrder = sortOrder
        };

        var result = await claimService.GetClaimsAsync(filters);
        return Ok(result);
    }

    [HttpGet("{claimID:int}")]
    public async Task<IActionResult> GetClaimDetail(int claimID)
    {
        var result = await claimService.GetClaimDetailAsync(claimID);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("{claimID:int}/summary")]
    public async Task<IActionResult> GetClaimSummary(int claimID)
    {
        var result = await claimService.GetClaimSummaryAsync(claimID);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPatch("{claimID:int}/status")]
    public async Task<IActionResult> UpdateStatus(int claimID, [FromBody] UpdateClaimStatusRequestDto dto)
    {
        try
        {
            var result = await claimService.UpdateClaimStatusAsync(claimID, dto);
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
}

