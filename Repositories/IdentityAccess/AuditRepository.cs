using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Models;
using Telebill.Dto.IdentityAccess;

namespace Telebill.Repositories.IdentityAccess
{
    public class AuditRepository : IAuditRepository
    {
        private readonly TeleBillContext tb;

        public AuditRepository(TeleBillContext _tb)
        {
            tb = _tb ?? throw new ArgumentNullException(nameof(_tb));
        }

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

        tb.AuditLogs.Add(entity);
        await tb.SaveChangesAsync();
        }

        
        public Task<IEnumerable<AuditLogDTO>> GetallAsync()
        {
            throw new NotImplementedException("GetallAsync is implemented at controller level as requested.");
        }
    }
}
