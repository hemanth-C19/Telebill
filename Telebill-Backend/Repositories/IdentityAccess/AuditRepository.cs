using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        
        public Task<IEnumerable<AuditLogDTO>> GetallAsync()
        {
            throw new NotImplementedException("GetallAsync is implemented at controller level as requested.");
        }
    }
}
