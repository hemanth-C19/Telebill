using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Models;
using Telebill.Repositories.MasterData;

namespace Telebill.Services.MasterData
{
    public class MasterdataService : IMasterdataService
    {
        private readonly IMasterdataRepository _repo;

        public MasterdataService()
        {
            _repo = new MasterdataRepository();
        }

        public Task<IEnumerable<Provider>> GetAllProvidersAsync() => _repo.GetAllProvidersAsync();

        public Task<Provider> RegisterProviderAsync(Provider provider) => _repo.RegisterProviderAsync(provider);

        public Task<Provider?> UpdateProviderTelehealthAsync(int id, bool enrolled) =>
            _repo.UpdateProviderTelehealthAsync(id, enrolled);

        public Task<IEnumerable<Payer>> GetAllPayersAsync() => _repo.GetAllPayersAsync();

        public Task<Payer> AddPayerAsync(Payer payer) => _repo.AddPayerAsync(payer);

        public Task<PayerPlan?> AddPlanAsync(int payerId, PayerPlan plan) => _repo.AddPlanAsync(payerId, plan);

        public Task<decimal?> LookupFeeAsync(string cptCode, int planId) => _repo.LookupFeeAsync(cptCode, planId);

        public Task UploadFeeSchedulesAsync(IEnumerable<FeeSchedule> rates) => _repo.UploadFeeSchedulesAsync(rates);
    }
}