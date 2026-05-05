using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Models;

namespace Telebill.Repositories.MasterData;

public interface IFeeScheduleRepository
{
    Task<List<FeeSchedule>> GetByPlanIdAsync(int planId);
    Task<FeeSchedule?> GetFeeByIdAsync(int feeId);
    Task<bool> ExistsAsync(int feeId);
    Task AddAsync(FeeSchedule fee);
    Task UpdateAsync(FeeSchedule fee);
    Task DeleteAsync(FeeSchedule fee);
    Task<bool> PlanExistsAsync(int planId);
}

