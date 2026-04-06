using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.MasterData;
using Telebill.Models;
using Telebill.Services.MasterData;

namespace Telebill.Controllers.MasterData;

[ApiController]
[Route("api/v1/MasterData/[controller]")]
public class PayerPlansController : ControllerBase
{
    private readonly IPayerPlanService _planService;

    public PayerPlansController(IPayerPlanService planService)
    {
        _planService = planService;
    }

    [HttpGet("GetPlansByPayerId")]
    public async Task<ActionResult<IEnumerable<PayerPlan>>> GetPlansByPayerId(int payerId)
    {
        try
        {
            var plans = await _planService.GetPlansByPayerIdAsync(payerId);
            return Ok(plans);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("GetPlanNamesByPayerId")]
    public async Task<ActionResult<IEnumerable<PlanNamesDTO>>> GetPlanNamesByPayerId(int payerId)
    {
        try
        {
            var planNames = await _planService.GetPlanNamesByPayerIdAsync(payerId);
            return Ok(planNames);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("AddPlan")]
    public async Task<IActionResult> AddPlan([FromBody] AddPayerPlanDTO plan)
    {
        try
        {
            await _planService.AddPlanAsync(plan);
            return Ok("Add Plan Successfull");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("UpdatePlan")]
    public async Task<IActionResult> UpdatePlan([FromBody] UpdatePayerPlanDTO plan)
    {
        try
        {
            await _planService.UpdatePlanAsync(plan);
            return Ok("Update plan Successfull");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("DeletePlan")]
    public async Task<IActionResult> DeletePlan(int planId)
    {
        try
        {
            await _planService.DeletePlanAsync(planId);
            return Ok("Delete Plan Successfull");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}

