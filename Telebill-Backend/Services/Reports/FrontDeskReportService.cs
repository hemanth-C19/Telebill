using System.Threading.Tasks;
using Telebill.Dto.Reports;
using Telebill.Repositories.Reports;

namespace Telebill.Services.Reports;

public class FrontDeskReportService(IReportQueryRepository queryRepo) : IFrontDeskReportService
{
    public Task<FrontDeskSummaryDto> GetSummaryAsync() => queryRepo.GetFrontDeskSummaryAsync();
}
