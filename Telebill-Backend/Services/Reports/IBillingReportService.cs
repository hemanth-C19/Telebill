using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.Reports;

namespace Telebill.Services.Reports;

public interface IBillingReportService
{
    // Fetch raw data, call KpiService, store result in BillingReport table
    Task<(bool success, string error, KpiResultDto? result)> GenerateAndStoreAsync(
        GenerateReportRequestDto dto);

    // List stored BillingReport rows
    Task<List<BillingReportListItemDto>> GetAllAsync(BillingReportFilterParams filters);

    // Get one stored report + parse MetricsJson into detail DTO
    Task<BillingReportDetailDto?> GetByIdAsync(int reportId);
}

