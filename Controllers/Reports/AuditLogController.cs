using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.Reports;
using Telebill.Services.Reports;

namespace Telebill.Controllers.Reports;

[ApiController]
[Route("api/v1/reports/audit")]
[Authorize(Roles = "Admin")]
public class AuditLogController(IAuditSearchService auditSearchService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] int? userId,
        [FromQuery] string? action,
        [FromQuery] string? resource,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var filters = new AuditSearchParams
        {
            UserId = userId,
            Action = action,
            Resource = resource,
            DateFrom = dateFrom,
            DateTo = dateTo,
            Page = page,
            PageSize = pageSize
        };

        var result = await auditSearchService.SearchAsync(filters);
        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] int? userId,
        [FromQuery] string? action,
        [FromQuery] string? resource,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        var filters = new AuditSearchParams
        {
            UserId = userId,
            Action = action,
            Resource = resource,
            DateFrom = dateFrom,
            DateTo = dateTo
        };

        var rows = await auditSearchService.ExportAsync(filters);
        return Ok(rows);
    }
}

