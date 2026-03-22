using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto;
using Telebill.Services.ChargeLines;

namespace Telebill.Controllers
{
    [ApiController]
    [Route("EncounterModule/[controller]")]
    public class ChargeLineController(IChargeLineService service) : ControllerBase
    {
        // GET: EncounterModule/ChargeLine/ByEncounter/123
        [HttpGet("ByEncounter/{encounterId:int}")]
        public async Task<IActionResult> GetByEncounterId([FromRoute] int encounterId, CancellationToken ct)
        {
            try
            {
                var items = await service.GetByEncounterId(encounterId);
                return Ok(items);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: EncounterModule/ChargeLine/123
        [HttpGet("{chargeId:int}")]
        public async Task<IActionResult> GetById([FromRoute] int chargeId, CancellationToken ct)
        {
            try
            {
                var item = await service.GetById(chargeId);
                if (item is null) return NotFound();
                return Ok(item);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // POST: EncounterModule/ChargeLine/Add/encounter/123
        [HttpPost("Add/encounter/{encounterId:int}")]
        public async Task<IActionResult> Add([FromRoute] int encounterId, [FromBody] ChargeLineCreateDto dto, CancellationToken ct)
        {
            try
            {
                var created = await service.Add(encounterId, dto);
                // Route to self by id
                return CreatedAtAction(nameof(GetById), new { chargeId = created.ChargeId }, created);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // PUT: EncounterModule/ChargeLine/123
        [HttpPut("{chargeId:int}")]
        public async Task<IActionResult> Update([FromRoute] int chargeId, [FromBody] ChargeLineUpdateDto dto, CancellationToken ct)
        {
            try
            {
                var updated = await service.Update(chargeId, dto);
                if (updated is null) return NotFound();
                return Ok(updated);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // DELETE: EncounterModule/ChargeLine/123
        [HttpDelete("{chargeId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int chargeId, CancellationToken ct)
        {
            try
            {
                var ok = await service.Delete(chargeId);
                return ok ? NoContent() : NotFound();
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // PUT: EncounterModule/ChargeLine/123/status?value=Finalized
        [HttpPut("{chargeId:int}/status")]
        public async Task<IActionResult> SetStatus([FromRoute] int chargeId, [FromQuery] string value, CancellationToken ct)
        {
            try
            {
                var ok = await service.SetStatus(chargeId, value);
                return ok ? NoContent() : NotFound();
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}