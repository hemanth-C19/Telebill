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

        public Task<IEnumerable<AuditLogDTO>> GetTopAsync(int count)
        {
            return auditRepository.GetTopAsync(count);
        }

        public Task DeleteAuditsAsync()
        {
            return auditRepository.DeleteAsync();
        }
    }
}