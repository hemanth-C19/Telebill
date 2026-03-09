using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto;
using Telebill.Services.Attestations;

namespace Telebill.Controllers
{
    [ApiController]
    [Route("EncounterModule/[controller]")]
    public class AttestationController : ControllerBase
    {
        private readonly IAttestationService _service;

        public AttestationController(IAttestationService service)
        {
            _service = service;
        }

        // GET: EncounterModule/Attestation/ByEncounter/123
        [HttpGet("ByEncounter/{encounterId:int}")]
        public async Task<IActionResult> GetByEncounterId([FromRoute] int encounterId, CancellationToken ct)
        {
            try
            {
                var item = await _service.GetByEncounterId(encounterId);
                if (item is null) return NotFound();
                return Ok(item);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: EncounterModule/Attestation/123
        [HttpGet("{attestId:int}")]
        public async Task<IActionResult> GetById([FromRoute] int attestId, CancellationToken ct)
        {
            try
            {
                var item = await _service.GetById(attestId);
                if (item is null) return NotFound();
                return Ok(item);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // POST: EncounterModule/Attestation/Add/encounter/123
        [HttpPost("Add/encounter/{encounterId:int}")]
        public async Task<IActionResult> Add([FromRoute] int encounterId, [FromBody] AttestationCreateDto dto, CancellationToken ct)
        {
            try
            {
                var created = await _service.Add(encounterId, dto);
                return CreatedAtAction(nameof(GetById), new { attestId = created.AttestId }, created);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // PUT: EncounterModule/Attestation/123
        [HttpPut("{attestId:int}")]
        public async Task<IActionResult> Update([FromRoute] int attestId, [FromBody] AttestationUpdateDto dto, CancellationToken ct)
        {
            try
            {
                var updated = await _service.Update(attestId, dto);
                if (updated is null) return NotFound();
                return Ok(updated);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // DELETE: EncounterModule/Attestation/123
        [HttpDelete("{attestId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int attestId, CancellationToken ct)
        {
            try
            {
                var ok = await _service.Delete(attestId);
                return ok ? NoContent() : NotFound();
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // PUT: EncounterModule/Attestation/123/finalize?status=Signed
        [HttpPut("{attestId:int}/finalize")]
        public async Task<IActionResult> Finalize([FromRoute] int attestId, [FromQuery] string? status, CancellationToken ct)
        {
            try
            {
                var ok = await _service.Finalize(attestId, DateTime.UtcNow, status ?? "Signed");
                return ok ? NoContent() : NotFound();
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}