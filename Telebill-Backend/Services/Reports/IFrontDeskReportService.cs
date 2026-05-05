using System.Threading.Tasks;
using Telebill.Dto.Reports;

namespace Telebill.Services.Reports;

public interface IFrontDeskReportService
{
    Task<FrontDeskSummaryDto> GetSummaryAsync();
}
