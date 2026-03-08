using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.MasterData;
using Telebill.Models;

namespace Telebill.Repositories.MasterData
{
    public interface IProviderRepository
    {
        Task<IEnumerable<Provider>> GetAllProvidersAsync();
        Task RegisterProviderAsync(CreateUpdateProviderDTO provider);
        Task UpdateProviderTelehealthAsync(int Pid, CreateUpdateProviderDTO dto);
        Task<Provider> GetProviderByNPIAsync(string NpiId);
        Task<Provider> GetProviderByNameAsync(string ProviderName);
        Task<IEnumerable<ProviderActiveInfo>> GetActiveProvidersAsync();

        Task DeleteProviderByIdAsync(int Pid);
    }
}