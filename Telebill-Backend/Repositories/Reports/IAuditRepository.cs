using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.Reports;
using Telebill.Models;

namespace Telebill.Repositories.Reports;

public interface IAuditRepository
{
    // Paged search — returns records AND total count
    Task<(List<AuditLog> Items, int TotalCount)> SearchAsync(AuditSearchParams filters);

    // Unpaged export — same filters but no pagination limit
    Task<List<AuditLog>> ExportAsync(AuditSearchParams filters);

    // Resolve user name for display
    Task<string?> GetUserNameByIdAsync(int userId);
}

