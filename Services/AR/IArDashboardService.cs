using System.Threading.Tasks;
using Telebill.Dto.AR;

namespace Telebill.Services.AR;

public interface IArDashboardService
{
    Task<ArDashboardSummaryDto> GetArDashboardAsync();
}

