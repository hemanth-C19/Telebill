using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.Coding;
using Telebill.Services.Coding;

namespace Telebill.Controllers
{
    [ApiController]
    [Route("api/v1/coding/diagnoses")]
    [Authorize(Roles = "Coder,Admin")]
    public class DiagnosisController(ICoderWorklistService service) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> AddDiagnosis(
            [FromBody] AddDiagnosisDto dto,
            [FromQuery] int userId)
        {
            var (success, error, result) = await service.AddDiagnosisAsync(dto, userId);
            if (!success)
            {
                return BadRequest(error);
            }

            return CreatedAtAction(nameof(GetByEncounter), new { encounterId = dto.EncounterId }, result);
        }

        [HttpGet("by-encounter/{encounterId:int}")]
        public async Task<ActionResult<List<DiagnosisResultDto>>> GetByEncounter(int encounterId)
        {
            var list = await service.GetDiagnosesByEncounterAsync(encounterId);
            return Ok(list);
        }

        [HttpPatch("{dxId:int}")]
        public async Task<IActionResult> UpdateDiagnosis(
            int dxId,
            [FromBody] UpdateDiagnosisDto dto,
            [FromQuery] int userId)
        {
            var (success, error, result) = await service.UpdateDiagnosisAsync(dxId, dto, userId);
            if (!success)
            {
                return BadRequest(error);
            }

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpDelete("{dxId:int}")]
        public async Task<IActionResult> RemoveDiagnosis(
            int dxId,
            [FromQuery] int userId)
        {
            var (success, error) = await service.RemoveDiagnosisAsync(dxId, userId);
            if (!success)
            {
                return BadRequest(error);
            }

            return NoContent();
        }
    }
}

