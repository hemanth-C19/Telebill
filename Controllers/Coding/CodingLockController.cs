using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.Coding;
using Telebill.Services.Coding;

namespace Telebill.Controllers
{
    [ApiController]
    [Route("api/v1/coding/lock")]
    public class CodingLockController : ControllerBase
    {
        private readonly ICodingLockService _service;

        public CodingLockController(ICodingLockService service)
        {
            _service = service;
        }

        [HttpGet("validate/{encounterId:int}")]
        public async Task<ActionResult<CodingValidationResultDto>> Validate(int encounterId)
        {
            var result = await _service.ValidateCodingLockAsync(encounterId);
            return Ok(result);
        }

        [HttpPost("apply")]
        public async Task<ActionResult<ApplyCodingLockResponseDto>> Apply(
            [FromBody] ApplyCodingLockDto dto,
            [FromQuery] int userId)
        {
            var result = await _service.ApplyCodingLockAsync(dto, userId);
            return Ok(result);
        }

        [HttpPost("unlock")]
        public async Task<IActionResult> Unlock(
            [FromBody] UnlockCodingDto dto,
            [FromQuery] int userId)
        {
            var (success, error, result) = await _service.UnlockCodingAsync(dto, userId);
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

        [HttpGet("history/{encounterId:int}")]
        public async Task<ActionResult<List<CodingLockResultDto>>> History(int encounterId)
        {
            var list = await _service.GetCodingLockHistoryAsync(encounterId);
            return Ok(list);
        }
    }
}

