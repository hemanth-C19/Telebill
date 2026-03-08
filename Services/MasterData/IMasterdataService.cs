using Telebill.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Telebill.Services.MasterData
{
    public interface IMasterdataService
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