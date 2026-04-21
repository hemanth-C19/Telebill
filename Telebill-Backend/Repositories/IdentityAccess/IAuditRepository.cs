using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.IdentityAccess;

namespace Telebill.Repositories.IdentityAccess
{
    public interface IAuditRepository
    {
        Task AddAsync(AuditLogDTO auditLogDTO);
        Task<IEnumerable<AuditLogDTO>> GetTopAsync(int count);
    }
}