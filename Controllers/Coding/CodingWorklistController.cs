using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.Coding;
using Telebill.Services.Coding;

namespace Telebill.Controllers
{
    [ApiController]
    [Route("api/v1/coding/worklist")]
    [Authorize(Roles = "Coder,Admin")]
    public class CodingWorklistController(ICoderWorklistService service) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<CodingWorklistItemDto>>> GetWorklist(
            [FromQuery] int? providerId,
            [FromQuery] int? planId)
        {
            var items = await service.GetCodingWorklistAsync(providerId, planId);
            return Ok(items);
        }

        [HttpGet("{encounterId:int}")]
        public async Task<IActionResult> GetEncounterCard(int encounterId)
        {
            var card = await service.GetCodingEncounterCardAsync(encounterId);
            if (card == null)
            {
                return NotFound();
            }

            return Ok(card);
        }

        [HttpPatch("{encounterId:int}/pos")]
        public async Task<IActionResult> UpdateEncounterPos(
            int encounterId,
            [FromBody] UpdateEncounterPosDto dto,
            [FromQuery] int userId)
        {
            var (success, error) = await service.UpdateEncounterPosAsync(encounterId, dto, userId);
            if (!success)
            {
                return BadRequest(error);
            }

            return Ok();
        }

        [HttpPatch("{encounterId:int}/charge-lines/{chargeId:int}/modifiers")]
        public async Task<IActionResult> UpdateChargeLineModifiers(
            int encounterId,
            int chargeId,
            [FromBody] UpdateChargeLineModifiersDto dto,
            [FromQuery] int userId)
        {
            var (success, error, updated) =
                await service.UpdateChargeLineModifiersAsync(encounterId, chargeId, dto, userId);

            if (!success)
            {
                return BadRequest(error);
            }

            if (updated == null)
            {
                return NotFound();
            }

            return Ok(updated);
        }
    }
}

