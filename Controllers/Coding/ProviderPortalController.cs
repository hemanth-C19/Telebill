using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.Coding;
using Telebill.Services.Coding;

namespace Telebill.Controllers
{
    [ApiController]
    [Route("api/v1/coding/provider")]
    public class ProviderPortalController : ControllerBase
    {
        private readonly IProviderCodingService _service;

        public ProviderPortalController(IProviderCodingService service)
        {
            _service = service;
        }

        [HttpGet("encounters")]
        public async Task<ActionResult<List<ProviderEncounterSummaryDto>>> GetProviderEncounters(
            [FromQuery] int providerId,
            [FromQuery] string? status)
        {
            var items = await _service.GetProviderEncountersAsync(providerId, status);
            return Ok(items);
        }

        [HttpGet("encounters/{encounterId:int}")]
        public async Task<IActionResult> GetProviderEncounterDetail(
            int encounterId,
            [FromQuery] int providerId)
        {
            var detail = await _service.GetProviderEncounterDetailAsync(encounterId, providerId);
            if (detail == null)
            {
                return NotFound();
            }

            return Ok(detail);
        }

        [HttpPatch("encounters/{encounterId:int}/documentation")]
        public async Task<IActionResult> SetDocumentationUri(
            int encounterId,
            [FromBody] SetDocumentationUriDto dto,
            [FromQuery] int providerId)
        {
            var updated = await _service.SetDocumentationUriAsync(encounterId, dto, providerId);
            if (updated == null)
            {
                return NotFound();
            }

            return Ok(updated);
        }

        [HttpPost("encounters/{encounterId:int}/ready-for-coding")]
        public async Task<IActionResult> MarkReadyForCoding(
            int encounterId,
            [FromQuery] int providerId)
        {
            var (success, error) = await _service.MarkReadyForCodingAsync(encounterId, providerId);
            if (!success)
            {
                return BadRequest(error);
            }

            var detail = await _service.GetProviderEncounterDetailAsync(encounterId, providerId);
            return Ok(detail);
        }
    }
}

