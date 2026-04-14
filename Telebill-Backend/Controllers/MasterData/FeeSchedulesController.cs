using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.MasterData;
using Telebill.Models;
using Telebill.Services.MasterData;

namespace Telebill.Controllers.MasterData;

[ApiController]
[Route("api/v1/MasterData/[controller]")]
[Authorize(Roles = "FrontDesk,Coder,Provider,AR,Admin")]
public class FeeSchedulesController : ControllerBase
{
    private readonly IFeeScheduleService _feeService;

    public FeeSchedulesController(IFeeScheduleService feeService)
    {
        _feeService = feeService;
    }

    [HttpGet("GetFeesByPlanId")]
    public async Task<ActionResult<IEnumerable<FeeSchedule>>> GetFeesByPlanId(int planId)
    {
        try
        {
            var fees = await _feeService.GetFeesByPlanIdAsync(planId);
            return Ok(fees);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("AddFee")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddFee([FromBody] AddFeeDTO fee)
    {
        try
        {
            await _feeService.AddFeeAsync(fee);
            return Ok("Add Fee Successfull");
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

    [HttpPut("UpdateFee")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateFee([FromBody] UpdateFeeDTO fee)
    {
        try
        {
            await _feeService.UpdateFeeAsync(fee);
            return Ok("Update Fee Successfull");
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

    [HttpDelete("DeleteFee")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteFee(int feeId)
    {
        try
        {
            await _feeService.DeleteFeeAsync(feeId);
            return Ok("Delete Fee Successfull");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}

