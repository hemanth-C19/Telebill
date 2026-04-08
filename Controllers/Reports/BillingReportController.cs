using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.Reports;
using Telebill.Services.Reports;

namespace Telebill.Controllers.Reports;

[ApiController]
[Route("api/v1/reports/billing")]
public class BillingReportController(IBillingReportService billingReportService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetReports(
        [FromQuery] string? scope,
        [FromQuery] System.DateTime? generatedFrom,
        [FromQuery] System.DateTime? generatedTo)
    {
        var filters = new BillingReportFilterParams
        {
            Scope = scope,
            GeneratedFrom = generatedFrom,
            GeneratedTo = generatedTo
        };

        var items = await billingReportService.GetAllAsync(filters);
        return Ok(items);
    }

    [HttpGet("{reportId:int}")]
    public async Task<IActionResult> GetReport(int reportId)
    {
        var detail = await billingReportService.GetByIdAsync(reportId);
        if (detail == null)
        {
            return NotFound();
        }

        return Ok(detail);
    }
}

