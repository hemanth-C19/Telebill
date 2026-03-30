using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.MasterData;
using Telebill.Models;

namespace Telebill.Services.MasterData;

public interface IFeeScheduleService
{
    Task<IEnumerable<FeeSchedule>> GetFeesByPlanIdAsync(int planId);

    Task AddFeeAsync(FeeDTO fee);
    Task UpdateFeeAsync(FeeDTO fee);
    Task DeleteFeeAsync(int feeId);
}

