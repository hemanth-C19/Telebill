using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Services;
using Telebill.Dto;

namespace Telebill.Controllers;

[ApiController]
public class ScrubController(IClaimService claimService) : ControllerBase
{
    [HttpPost("api/claims/{claimID:int}/scrub")]
    public async Task<IActionResult> ScrubClaim(int claimID)
    {
        var result = await claimService.ScrubClaimAsync(claimID);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("api/claims/{claimID:int}/scrub-issues")]
    public async Task<IActionResult> GetIssues(int claimID, [FromQuery] string status = "all")
    {
        var result = await claimService.GetScrubIssuesAsync(claimID, status);
        return Ok(result);
    }

    [HttpPatch("api/claims/{claimID:int}/scrub-issues/{issueID:int}/resolve")]
    public async Task<IActionResult> ResolveIssue(int claimID, int issueID, [FromBody] ResolveIssueRequestDto dto)
    {
        try
        {
            var result = await claimService.ResolveIssueAsync(claimID, issueID, dto);
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

    [HttpPost("api/claims/scrub-batch")]
    public async Task<IActionResult> RunScrubBatch()
    {
        var result = await claimService.RunScrubBatchAsync();
        return Ok(result);
    }

    [HttpGet("api/scrub-rules")]
    public async Task<IActionResult> GetRules([FromQuery] string? severity, [FromQuery] string? status)
    {
        var result = await claimService.GetScrubRulesAsync(severity, status);
        return Ok(result);
    }

    [HttpPost("api/scrub-rules")]
    public async Task<IActionResult> CreateRule([FromBody] CreateScrubRuleRequestDto dto)
    {
        var result = await claimService.CreateScrubRuleAsync(dto);
        return StatusCode(201, result);
    }

    [HttpPatch("api/scrub-rules/{ruleID:int}")]
    public async Task<IActionResult> UpdateRule(int ruleID, [FromBody] UpdateScrubRuleRequestDto dto)
    {
        var result = await claimService.UpdateScrubRuleAsync(ruleID, dto);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}

