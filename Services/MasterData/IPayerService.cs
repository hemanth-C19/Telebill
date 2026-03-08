using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.MasterData;
using Telebill.Models;

namespace Telebill.Services.MasterData
{
    public interface IPayerService
    {
        Task<IEnumerable<Payer>> GetAllPayersAsync();
        Task<IEnumerable<PayerNamesDTO>> GetAllPayersNames();
        Task<IEnumerable<PayerPlan>> GetPlansByPayerIdAsync(int payerId);
        Task<IEnumerable<PlanNamesDTO>> GetPlanNamesByPayerIdAsync(int payerId);
        Task<IEnumerable<FeeSchedule>> GetFeesByPlanIdAsync(int planId);

        Task AddPayerAsync(PayerDTO payerDto);
        Task UpdatePayerAsync(PayerDTO payerDto);
        Task DeletePayerAsync(int payerId);

        Task AddPlanAsync(PayerPlanDTO plan);
        Task UpdatePlanAsync(PayerPlanDTO plan);
        Task DeletePlanAsync(int planId);

        Task AddFeeAsync(FeeDTO fee);
        Task UpdateFeeAsync(FeeDTO fee);
        Task DeleteFeeAsync(int feeId);
    }
}