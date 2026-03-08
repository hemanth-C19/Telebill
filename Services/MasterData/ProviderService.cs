using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.MasterData;
using Telebill.Models;
using Telebill.Repositories.MasterData;

namespace Telebill.Services.MasterData
{
    public class ProviderService : IProviderService
    {
        private readonly IProviderRepository _repo;

        public ProviderService(IProviderRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<Provider>> GetAllProvidersAsync() => _repo.GetAllProvidersAsync();

        public Task RegisterProviderAsync(CreateUpdateProviderDTO provider) => _repo.RegisterProviderAsync(provider);

        public Task UpdateProviderByIdAsync(int Pid, CreateUpdateProviderDTO dto) =>
            _repo.UpdateProviderTelehealthAsync(Pid, dto);

        public Task<IEnumerable<ProviderActiveInfo>> GetActiveProvidersAsync()
        {
            return _repo.GetActiveProvidersAsync();
        }

        Task<Provider> IProviderService.GetProviderByNameAsync(string ProviderName)
        {
            return _repo.GetProviderByNameAsync(ProviderName);
        }

        Task<Provider> IProviderService.GetProviderByNPIAsync(string NpiId)
        {
            return _repo.GetProviderByNPIAsync(NpiId);
        }

        public Task DeleteProviderByIdAsync(int Pid) => _repo.DeleteProviderByIdAsync(Pid);
    }
}