using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.IdentityAccess;
using Telebill.Services.IdentityAccess;

namespace Telebill.Controllers
{
    [ApiController]
    [Route("api/v1/IdentityAccess/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditController(IAuditService auditService) : ControllerBase
    {
        private const int TopCount = 20;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuditLogDTO>>> GetLatest()
        {
            var result = await auditService.GetTopAsync(TopCount);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AuditLogDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await auditService.AddAsync(dto);
            return CreatedAtAction(nameof(GetLatest), new { id = dto.AuditId }, dto);
        }
    }
}