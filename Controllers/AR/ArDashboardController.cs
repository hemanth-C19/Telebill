using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telebill.Services.AR;

namespace Telebill.Controllers.AR;

[ApiController]
[Route("api/v1/ar/dashboard")]
public class ArDashboardController(IArDashboardService dashboardService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var summary = await dashboardService.GetArDashboardAsync();
        return Ok(summary);
    }
}

