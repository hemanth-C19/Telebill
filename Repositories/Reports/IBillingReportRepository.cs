using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.Reports;
using Telebill.Models;

namespace Telebill.Repositories.Reports;

public interface IBillingReportRepository
{
    Task<List<BillingReport>> GetAllAsync(BillingReportFilterParams filters);
    Task<BillingReport?> GetByIdAsync(int reportId);
    Task AddAsync(BillingReport report);
}

