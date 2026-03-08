using Microsoft.AspNetCore.Mvc;
using Telebill.Services.MasterData;
using Telebill.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Telebill.Controllers
{
    [ApiController]
    [Route("api/masterdata")]
    public class MasterDataController : ControllerBase
    {
        private readonly IMasterdataService _service;

        public MasterDataController()
        {
            _service = new MasterdataService();
        }

        // Providers
        [HttpGet("providers")]
        public async Task<IActionResult> GetProviders()
        {
            var result = await _service.GetAllProvidersAsync();
            return Ok(result);
        }

        [HttpPost("providers")]
        public async Task<IActionResult> RegisterProvider([FromBody] Provider provider)
        {
            var result = await _service.RegisterProviderAsync(provider);
            return CreatedAtAction(nameof(GetProviders), new { id = result.ProviderId }, result);
        }

        [HttpPut("providers/{id}")]
        public async Task<IActionResult> UpdateProviderTelehealth(int id, [FromBody] bool enrolled)
        {
            var result = await _service.UpdateProviderTelehealthAsync(id, enrolled);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // Payers
        [HttpGet("payers")]
        public async Task<IActionResult> GetPayers()
        {
            var result = await _service.GetAllPayersAsync();
            return Ok(result);
        }

        [HttpPost("payers")]
        public async Task<IActionResult> AddPayer([FromBody] Payer payer)
        {
            var result = await _service.AddPayerAsync(payer);
            return CreatedAtAction(nameof(GetPayers), new { id = result.PayerId }, result);
        }

        [HttpPost("payers/{id}/plans")]
        public async Task<IActionResult> AddPlan(int id, [FromBody] PayerPlan plan)
        {
            var result = await _service.AddPlanAsync(id, plan);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // FeeSchedules
        [HttpGet("feeschedules/lookup")]
        public async Task<IActionResult> LookupFee([FromQuery] string cptCode, [FromQuery] int planId)
        {
            var price = await _service.LookupFeeAsync(cptCode, planId);
            if (price == null) return NotFound();
            return Ok(price);
        }

        [HttpPost("feeschedules")]
        public async Task<IActionResult> UploadFeeSchedules([FromBody] IEnumerable<FeeSchedule> rates)
        {
            await _service.UploadFeeSchedulesAsync(rates);
            return Ok();
        }
    }
}