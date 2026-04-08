using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.AR;
using Telebill.Services.AR;

namespace Telebill.Controllers.AR;

[ApiController]
[Route("api/v1/ar/worklist")]
public class ArWorklistController(IDenialService denialService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetWorklist(
        [FromQuery] string? denialStatus,
        [FromQuery] string? reasonCode,
        [FromQuery] int? payerId,
        [FromQuery] string? agingBucket,
        [FromQuery] DateTime? denialDateFrom,
        [FromQuery] DateTime? denialDateTo)
    {
        var filters = new ArWorklistFilterParams
        {
            DenialStatus = denialStatus,
            ReasonCode = reasonCode,
            PayerId = payerId,
            AgingBucket = agingBucket,
            DenialDateFrom = denialDateFrom,
            DenialDateTo = denialDateTo
        };

        var items = await denialService.GetArWorklistAsync(filters);
        return Ok(items);
    }

    [HttpGet("{denialId:int}")]
    public async Task<IActionResult> GetDenialDetail(int denialId)
    {
        var detail = await denialService.GetDenialDetailAsync(denialId);
        if (detail == null)
        {
            return NotFound();
        }

        return Ok(detail);
    }
}

