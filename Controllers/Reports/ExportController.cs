using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.Reports;
using Telebill.Services.Reports;

namespace Telebill.Controllers.Reports;

[ApiController]
[Route("api/v1/reports/export")]
public class ExportController(IExportService exportService) : ControllerBase
{
    private static ExportFilterParams BuildFilters(
        DateTime? dateFrom,
        DateTime? dateTo,
        int? payerId,
        int? planId,
        int? providerId,
        string? status)
    {
        return new ExportFilterParams
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            PayerId = payerId,
            PlanId = planId,
            ProviderId = providerId,
            Status = status
        };
    }

    [HttpGet("claims")]
    public async Task<IActionResult> GetClaims(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int? payerId,
        [FromQuery] int? planId,
        [FromQuery] int? providerId,
        [FromQuery] string? status)
    {
        var filters = BuildFilters(dateFrom, dateTo, payerId, planId, providerId, status);
        var rows = await exportService.GetClaimsListingAsync(filters);
        return Ok(rows);
    }

    [HttpGet("scrub-issues")]
    public async Task<IActionResult> GetScrubIssues(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int? payerId,
        [FromQuery] int? planId,
        [FromQuery] int? providerId,
        [FromQuery] string? status)
    {
        var filters = BuildFilters(dateFrom, dateTo, payerId, planId, providerId, status);
        var rows = await exportService.GetScrubIssuesAsync(filters);
        return Ok(rows);
    }

    [HttpGet("ar-aging")]
    public async Task<IActionResult> GetArAging(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int? payerId,
        [FromQuery] int? planId,
        [FromQuery] int? providerId,
        [FromQuery] string? status)
    {
        var filters = BuildFilters(dateFrom, dateTo, payerId, planId, providerId, status);
        var rows = await exportService.GetArAgingAsync(filters);
        return Ok(rows);
    }

    [HttpGet("statements")]
    public async Task<IActionResult> GetStatements(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int? payerId,
        [FromQuery] int? planId,
        [FromQuery] int? providerId,
        [FromQuery] string? status)
    {
        var filters = BuildFilters(dateFrom, dateTo, payerId, planId, providerId, status);
        var rows = await exportService.GetStatementsSummaryAsync(filters);
        return Ok(rows);
    }

    [HttpGet("remits")]
    public async Task<IActionResult> GetRemits(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int? payerId,
        [FromQuery] int? planId,
        [FromQuery] int? providerId,
        [FromQuery] string? status)
    {
        var filters = BuildFilters(dateFrom, dateTo, payerId, planId, providerId, status);
        var rows = await exportService.GetRemitSummaryAsync(filters);
        return Ok(rows);
    }
}

