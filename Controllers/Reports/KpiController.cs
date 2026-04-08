using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.Reports;
using Telebill.Services.Reports;

namespace Telebill.Controllers.Reports;

[ApiController]
[Route("api/v1/reports/kpi")]
[Authorize(Roles = "FrontDesk,Coder,Provider,AR,Admin")]
public class KpiController(IBillingReportService billingReportService) : ControllerBase
{
    [HttpPost("generate")]
    [Authorize(Roles = "AR,Admin")]
    public async Task<IActionResult> GenerateKpi([FromBody] GenerateReportRequestDto dto)
    {
        var (success, error, result) = await billingReportService.GenerateAndStoreAsync(dto);
        if (!success)
        {
            return BadRequest(error);
        }

        return Ok(result);
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest([FromQuery] string scope, [FromQuery] int? scopeId)
    {
        var filters = new BillingReportFilterParams
        {
            Scope = scope
        };

        var reports = await billingReportService.GetAllAsync(filters);
        var latest = reports
            .OrderByDescending(r => r.GeneratedDate)
            .FirstOrDefault();

        if (latest == null)
        {
            return NotFound();
        }

        var detail = await billingReportService.GetByIdAsync(latest.ReportId);
        if (detail == null)
        {
            return NotFound();
        }

        return Ok(detail);
    }
}

