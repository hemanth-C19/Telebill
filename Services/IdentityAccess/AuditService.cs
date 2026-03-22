using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Repositories.IdentityAccess;
using Telebill.Dto.IdentityAccess;

namespace Telebill.Services.IdentityAccess
{
    public class AuditService(IAuditRepository auditRepository) : IAuditService
    {
        public Task AddAsync(AuditLogDTO auditLogDTO)
        {
            return auditRepository.AddAsync(auditLogDTO);
        }

        // This will still call the repository and thus throw (since repo isn't implementing GetallAsync).
        // Left as-is so existing code compiles. Your controller will not use this.
        public Task<IEnumerable<AuditLogDTO>> GetallAsync()
        {
            return auditRepository.GetallAsync();
        }
    }
}