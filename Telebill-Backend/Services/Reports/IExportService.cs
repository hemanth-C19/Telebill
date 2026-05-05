using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.Reports;

namespace Telebill.Services.Reports;

public interface IExportService
{
    Task<List<ClaimsListingRowDto>> GetClaimsListingAsync(ExportFilterParams filters);
    Task<List<ScrubIssueExportRowDto>> GetScrubIssuesAsync(ExportFilterParams filters);
    Task<List<ArAgingRowDto>> GetArAgingAsync(ExportFilterParams filters);
    Task<List<StatementSummaryRowDto>> GetStatementsSummaryAsync(ExportFilterParams filters);
    Task<List<RemitSummaryRowDto>> GetRemitSummaryAsync(ExportFilterParams filters);
}

