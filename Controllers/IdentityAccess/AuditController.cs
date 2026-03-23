using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telebill.Data;
using Telebill.Dto.IdentityAccess;
using Telebill.Models;
using Telebill.Services.IdentityAccess;

namespace Telebill.Controllers
{
    [ApiController]
    [Route("api/v1/IdentityAccess/[controller]")]
    public class AuditController(IAuditService auditService, TeleBillContext tb) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuditLogDTO>>> GetAll()
        {
            var result = await tb.AuditLogs
                .AsNoTracking()
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new AuditLogDTO
                {
                    AuditId = a.AuditId,
                    UserId = a.UserId,
                    Action = a.Action,
                    Resource = a.Resource,
                    Timestamp = a.Timestamp,
                    Metadata = a.Metadata
                })
                .ToListAsync()
                .ConfigureAwait(false);

            return Ok(result);
        }

        
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AuditLogDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await auditService.AddAsync(dto);
            return CreatedAtAction(nameof(GetAll), new { id = dto.AuditId }, dto);
        }
        
    }
}