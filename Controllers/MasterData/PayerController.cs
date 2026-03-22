using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Telebill.Services.MasterData;
using Telebill.Dto.MasterData;
using Telebill.Models;
using System.Collections.Generic;

namespace Telebill.Controllers.MasterData
{
    [ApiController]
    [Route("api/[controller]")]
    public class PayerController(IPayerService payerService) : ControllerBase
    {
        [HttpGet("GetAllPayers")]
        public async Task<ActionResult<IEnumerable<PayerDTO>>> GetAllPayers()
        {
            var payers = await payerService.GetAllPayersAsync();
            return Ok(payers);
        }

        [HttpGet("GetAllPayersNames")]
        public async Task<ActionResult<IEnumerable<PayerNamesDTO>>> GetAllPayerNames()
        {
            var names = await payerService.GetAllPayersNames();
            return Ok(names);
        }

        [HttpGet("GetPlansByPayerId")]
        public async Task<ActionResult<IEnumerable<PayerPlan>>> GetPlansByPayerId(int payerId)
        {
            var plans = await payerService.GetPlansByPayerIdAsync(payerId);
            return Ok(plans);
        }

        [HttpGet("GetPlanNamesByPayerId")]
        public async Task<ActionResult<IEnumerable<PlanNamesDTO>>> GetPlanNamesByPayerId(int payerId)
        {
            var planNames = await payerService.GetPlanNamesByPayerIdAsync(payerId);
            return Ok(planNames);
        }

        [HttpGet("GetFeesByPlanId")]
        public async Task<ActionResult<IEnumerable<FeeSchedule>>> GetFeesByPlanId(int planId)
        {
            var fees = await payerService.GetFeesByPlanIdAsync(planId);
            return Ok(fees);
        }

        [HttpPost("AddPayer")]
        public async Task<IActionResult> AddPayer([FromBody] PayerDTO payerDto)
        {
            await payerService.AddPayerAsync(payerDto);
            return Ok("Add Payer Successfull");
        }

        [HttpPut("UpdatePayer")]
        public async Task<IActionResult> UpdatePayer([FromBody] PayerDTO payerDto)
        {
            await payerService.UpdatePayerAsync(payerDto);
            return Ok("Update Payer Successfull");
        }

        [HttpDelete("DeletePayer")]
        public async Task<IActionResult> DeletePayer(int payerId)
        {
            await payerService.DeletePayerAsync(payerId);
            return Ok("Delete Payer Successfull");
        }

        [HttpPost("AddPlan")]
        public async Task<IActionResult> AddPlan([FromBody] PayerPlanDTO plan)
        {
            await payerService.AddPlanAsync(plan);
            return Ok("Add Plan Successfull");
        }

        [HttpPut("UpdatePlan")]
        public async Task<IActionResult> UpdatePlan([FromBody] PayerPlanDTO plan)
        {
            await payerService.UpdatePlanAsync(plan);
            return Ok("Update plan Successfull");
        }
    
        [HttpDelete("DeletePlan")]
        public async Task<IActionResult> DeletePlan(int planId)
        {
            await payerService.DeletePlanAsync(planId);
            return Ok("Delete Plan Successfull");
        }

        [HttpPost("AddFee")]
        public async Task<IActionResult> AddFee([FromBody] FeeDTO fee)
        {
            await payerService.AddFeeAsync(fee);
            return Ok("Add Fee Successfull");
        }

        [HttpPut("UpdateFee")]
        public async Task<IActionResult> UpdateFee([FromBody] FeeDTO fee)
        {
            await payerService.UpdateFeeAsync(fee);
            return Ok("Update Fee Successfull");
        }

        [HttpDelete("DeleteFee")]
        public async Task<IActionResult> DeleteFee(int feeId)
        {
            await payerService.DeleteFeeAsync(feeId);
            return Ok("Delete Fee Successfull");
        }
    }
}