using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.MasterData;
using Telebill.Models;

namespace Telebill.Services.MasterData;

public interface IPayerPlanService
{
    Task<IEnumerable<PayerPlan>> GetPlansByPayerIdAsync(int payerId);
    Task<IEnumerable<PlanNamesDTO>> GetPlanNamesByPayerIdAsync(int payerId);

    Task AddPlanAsync(AddPayerPlanDTO plan);
    Task UpdatePlanAsync(UpdatePayerPlanDTO plan);
    Task DeletePlanAsync(int planId);
}

