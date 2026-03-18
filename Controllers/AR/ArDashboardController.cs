using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telebill.Services.AR;

namespace Telebill.Controllers.AR;

[ApiController]
[Route("api/v1/ar/dashboard")]
public class ArDashboardController : ControllerBase
{
    private readonly IArDashboardService _dashboardService;

    public ArDashboardController(IArDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var summary = await _dashboardService.GetArDashboardAsync();
        return Ok(summary);
    }
}

