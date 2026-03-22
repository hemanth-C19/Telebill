using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.MasterData;
using Telebill.Models;
using Telebill.Repositories.MasterData;

namespace Telebill.Services.MasterData
{
    public class ProviderService(IProviderRepository repo) : IProviderService
    {
        public Task<IEnumerable<Provider>> GetAllProvidersAsync() => repo.GetAllProvidersAsync();

        public Task RegisterProviderAsync(CreateUpdateProviderDTO provider) => repo.RegisterProviderAsync(provider);

        public Task UpdateProviderByIdAsync(int Pid, CreateUpdateProviderDTO dto) =>
            repo.UpdateProviderTelehealthAsync(Pid, dto);

        public Task<IEnumerable<ProviderActiveInfo>> GetActiveProvidersAsync()
        {
            return repo.GetActiveProvidersAsync();
        }

        Task<Provider> IProviderService.GetProviderByNameAsync(string ProviderName)
        {
            return repo.GetProviderByNameAsync(ProviderName);
        }

        Task<Provider> IProviderService.GetProviderByNPIAsync(string NpiId)
        {
            return repo.GetProviderByNPIAsync(NpiId);
        }

        public Task DeleteProviderByIdAsync(int Pid) => repo.DeleteProviderByIdAsync(Pid);
    }
}