using System;
using Telebill.Dto.MasterData;
using Telebill.Models;
using Telebill.Repositories.MasterData;

namespace Telebill.Services.MasterData
{
    public class PayerService : IPayerService
    {
        private readonly IPayerRepository _payerRepository;

        public PayerService(IPayerRepository payerRepository)
        {
            _payerRepository = payerRepository;
        }

        public async Task<IEnumerable<Payer>> GetAllPayersAsync()
        {
            return await _payerRepository.GetAllPayersAsync();
        }

        public async Task<IEnumerable<PayerNamesDTO>> GetAllPayersNames()
        {
            return await _payerRepository.GetAllPayersNames();
        }

        public async Task<IEnumerable<PayerPlan>> GetPlansByPayerIdAsync(int payerId)
        {
            return await _payerRepository.GetPlansByPayerIdAsync(payerId);
        }

        public async Task<IEnumerable<PlanNamesDTO>> GetPlanNamesByPayerIdAsync(int payerId){
            return await _payerRepository.GetPlanNamesByPayerIdAsync(payerId);
        }

        public async Task<IEnumerable<FeeSchedule>> GetFeesByPlanIdAsync(int planId)
        {
            return await _payerRepository.GetFeesByPlanIdAsync(planId);
        }

        public async Task AddPayerAsync(PayerDTO payerDto)
        {
            await _payerRepository.AddPayerAsync(payerDto);
        }

        public async Task UpdatePayerAsync(PayerDTO payerDto)
        {
            await _payerRepository.UpdatePayerAsync(payerDto);
        }

        public async Task DeletePayerAsync(int payerId)
        {
            await _payerRepository.DeletePayerAsync(payerId);
        }

        public async Task AddPlanAsync(PayerPlanDTO plan)
        {
            await _payerRepository.AddPlanAsync(plan);
        }

        public async Task UpdatePlanAsync(PayerPlanDTO plan)
        {
            await _payerRepository.UpdatePlanAsync(plan);
        }

        public async Task DeletePlanAsync(int planId)
        {
            await _payerRepository.DeletePlanAsync(planId);
        }

        public async Task AddFeeAsync(FeeDTO fee)
        {
            await _payerRepository.AddFeeAsync(fee);
        }

        public async Task UpdateFeeAsync(FeeDTO fee)
        {
            await _payerRepository.UpdateFeeAsync(fee);
        }

        public async Task DeleteFeeAsync(int feeId)
        {
            await _payerRepository.DeleteFeeAsync(feeId);
        }
    }
}