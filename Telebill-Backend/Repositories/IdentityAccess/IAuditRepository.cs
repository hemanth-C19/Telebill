using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.IdentityAccess;

namespace Telebill.Repositories.IdentityAccess
{
    public interface IAuditRepository
    {
        Task AddAsync(AuditLogDTO auditLogDTO);

        // Keeping the signature as-is, but *not* implementing in repo per your request.
        Task<IEnumerable<AuditLogDTO>> GetallAsync();
    }
}