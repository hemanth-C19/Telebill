using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telebill.Services.AR;

namespace Telebill.Controllers.AR;

[ApiController]
[Route("api/v1/ar/dashboard")]
[Authorize(Roles = "AR,Admin")]
public class ArDashboardController(IArDashboardService dashboardService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var summary = await dashboardService.GetArDashboardAsync();
        return Ok(summary);
    }
}

