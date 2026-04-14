using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Models;

namespace Telebill.Repositories.MasterData
{
    public interface IProviderRepository
    {
        Task<IEnumerable<Provider>> GetAllProvidersAsync(string? search, int page, int limit);
        Task<Provider?> GetProviderByIdAsync(int providerId);
        Task<Provider?> GetProviderByNPIAsync(string npiId);
        Task<Provider?> GetProviderByNameAsync(string providerName);
        Task<List<Provider>> GetActiveProvidersAsync();

        Task AddProviderAsync(Provider provider);
        Task UpdateProviderAsync(Provider provider);

        Task DeleteProviderAsync(Provider provider);
    }
}