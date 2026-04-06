using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.Reports;
using Telebill.Services.Reports;

namespace Telebill.Controllers.Reports;

[ApiController]
[Route("api/v1/reports/kpi")]
public class KpiController : ControllerBase
{
    private readonly IBillingReportService _billingReportService;

    public KpiController(IBillingReportService billingReportService)
    {
        _billingReportService = billingReportService;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateKpi([FromBody] GenerateReportRequestDto dto)
    {
        var (success, error, result) = await _billingReportService.GenerateAndStoreAsync(dto);
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

        var reports = await _billingReportService.GetAllAsync(filters);
        var latest = reports
            .OrderByDescending(r => r.GeneratedDate)
            .FirstOrDefault();

        if (latest == null)
        {
            return NotFound();
        }

        var detail = await _billingReportService.GetByIdAsync(latest.ReportId);
        if (detail == null)
        {
            return NotFound();
        }

        return Ok(detail);
    }
}

