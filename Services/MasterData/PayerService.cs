using System;
using Telebill.Dto.MasterData;
using Telebill.Models;
using Telebill.Repositories.MasterData;

namespace Telebill.Services.MasterData
{
    public class PayerService(IPayerRepository payerRepository) : IPayerService
    {
        public async Task<IEnumerable<Payer>> GetAllPayersAsync()
        {
            return await payerRepository.GetAllPayersAsync();
        }

        public async Task<IEnumerable<PayerNamesDTO>> GetAllPayersNames()
        {
            return await payerRepository.GetAllPayersNames();
        }

        public async Task<IEnumerable<PayerPlan>> GetPlansByPayerIdAsync(int payerId)
        {
            return await payerRepository.GetPlansByPayerIdAsync(payerId);
        }

        public async Task<IEnumerable<PlanNamesDTO>> GetPlanNamesByPayerIdAsync(int payerId){
            return await payerRepository.GetPlanNamesByPayerIdAsync(payerId);
        }

        public async Task<IEnumerable<FeeSchedule>> GetFeesByPlanIdAsync(int planId)
        {
            return await payerRepository.GetFeesByPlanIdAsync(planId);
        }

        public async Task AddPayerAsync(PayerDTO payerDto)
        {
            await payerRepository.AddPayerAsync(payerDto);
        }

        public async Task UpdatePayerAsync(PayerDTO payerDto)
        {
            await payerRepository.UpdatePayerAsync(payerDto);
        }

        public async Task DeletePayerAsync(int payerId)
        {
            await payerRepository.DeletePayerAsync(payerId);
        }

        public async Task AddPlanAsync(PayerPlanDTO plan)
        {
            await payerRepository.AddPlanAsync(plan);
        }

        public async Task UpdatePlanAsync(PayerPlanDTO plan)
        {
            await payerRepository.UpdatePlanAsync(plan);
        }

        public async Task DeletePlanAsync(int planId)
        {
            await payerRepository.DeletePlanAsync(planId);
        }

        public async Task AddFeeAsync(FeeDTO fee)
        {
            await payerRepository.AddFeeAsync(fee);
        }

        public async Task UpdateFeeAsync(FeeDTO fee)
        {
            await payerRepository.UpdateFeeAsync(fee);
        }

        public async Task DeleteFeeAsync(int feeId)
        {
            await payerRepository.DeleteFeeAsync(feeId);
        }
    }
}