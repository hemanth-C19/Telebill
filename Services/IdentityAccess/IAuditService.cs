using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.IdentityAccess;

namespace Telebill.Services.IdentityAccess
{
    public interface IAuditService
    {
        Task AddAsync(AuditLogDTO auditLogDTO);
        Task<IEnumerable<AuditLogDTO>> GetallAsync(); // Kept for compatibility; will throw if called
    }
}