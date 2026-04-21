using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telebill.Models;
using Telebill.Dto.IdentityAccess;
using Telebill.Data;

namespace Telebill.Repositories.IdentityAccess
{
    public class AuditRepository(TeleBillContext tb) : IAuditRepository
    {
        private readonly TeleBillContext _tb = tb ?? throw new ArgumentNullException(nameof(tb));

        public async Task AddAsync(AuditLogDTO auditLogDTO)
        {
            var entity = new AuditLog
            {
                UserId = auditLogDTO.UserId,
                Action = auditLogDTO.Action,
                Resource = auditLogDTO.Resource,
                Timestamp = auditLogDTO.Timestamp,
                Metadata = auditLogDTO.Metadata
            };

            _tb.AuditLogs.Add(entity);
            await _tb.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuditLogDTO>> GetTopAsync(int count)
        {
            return await _tb.AuditLogs
                .AsNoTracking()
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
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
        }
    }
}
