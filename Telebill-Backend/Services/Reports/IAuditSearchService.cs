using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.Reports;

namespace Telebill.Services.Reports;

public interface IAuditSearchService
{
    Task<AuditLogPagedResultDto> SearchAsync(AuditSearchParams filters);
    Task<List<AuditLogRowDto>> ExportAsync(AuditSearchParams filters);
}

