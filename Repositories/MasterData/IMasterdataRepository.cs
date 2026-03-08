using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Models;

namespace Telebill.Repositories.MasterData
{
    public interface IMasterdataRepository
    {
        Task<IEnumerable<Provider>> GetAllProvidersAsync();
        Task<Provider> RegisterProviderAsync(Provider provider);
        Task<Provider?> UpdateProviderTelehealthAsync(int id, bool enrolled);

        Task<IEnumerable<Payer>> GetAllPayersAsync();
        Task<Payer> AddPayerAsync(Payer payer);
        Task<PayerPlan?> AddPlanAsync(int payerId, PayerPlan plan);

        Task<decimal?> LookupFeeAsync(string cptCode, int planId);
        Task UploadFeeSchedulesAsync(IEnumerable<FeeSchedule> rates);
    }
}