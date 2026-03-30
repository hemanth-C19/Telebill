using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Dto.MasterData;
using Telebill.Models;

namespace Telebill.Services.MasterData
{
    public interface IProviderService
    {
        Task<IEnumerable<Provider>> GetAllProvidersAsync();
        Task<Provider?> GetProviderByNPIAsync(string npiId);
        Task<Provider?> GetProviderByNameAsync(string providerName);
        Task<IEnumerable<ProviderActiveInfo>> GetActiveProvidersAsync();

        Task RegisterProviderAsync(CreateUpdateProviderDTO provider);
        Task UpdateProviderByIdAsync(int providerId, CreateUpdateProviderDTO dto);
        Task DeleteProviderByIdAsync(int providerId);
    }
}