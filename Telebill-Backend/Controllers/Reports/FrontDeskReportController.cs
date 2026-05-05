using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telebill.Services.Reports;

namespace Telebill.Controllers.Reports;

[ApiController]
[Route("api/v1/reports/frontdesk")]
[Authorize(Roles = "FrontDesk,Admin")]
public class FrontDeskReportController(IFrontDeskReportService service) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var result = await service.GetSummaryAsync();
        return Ok(result);
    }
}
