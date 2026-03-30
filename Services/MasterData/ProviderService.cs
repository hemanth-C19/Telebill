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

        public Task<Provider?> GetProviderByNPIAsync(string npiId) => _repo.GetProviderByNPIAsync(npiId);

        public Task<Provider?> GetProviderByNameAsync(string providerName) => _repo.GetProviderByNameAsync(providerName);

        public async Task<IEnumerable<ProviderActiveInfo>> GetActiveProvidersAsync()
        {
            var providers = await _repo.GetActiveProvidersAsync();
            return providers.ConvertAll(p => new ProviderActiveInfo
            {
                ProviderId = p.ProviderId,
                ProviderName = p.Name
            });
        }

        public async Task RegisterProviderAsync(CreateUpdateProviderDTO provider)
        {
            if (string.IsNullOrWhiteSpace(provider.ProviderName) || string.IsNullOrWhiteSpace(provider.ProviderNpi))
            {
                throw new ArgumentException("ProviderName and ProviderNpi are required.");
            }

            var entity = new Provider
            {
                Name = provider.ProviderName,
                Npi = provider.ProviderNpi,
                Taxonomy = provider.ProviderTaxonomy,
                TelehealthEnrolled = provider.ProviderEnrolled,
                ContactInfo = provider.ProviderContact,
                Status = provider.ProviderStatus
            };

            await _repo.AddProviderAsync(entity);
        }

        public async Task UpdateProviderByIdAsync(int providerId, CreateUpdateProviderDTO dto)
        {
            var existing = await _repo.GetProviderByIdAsync(providerId);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Provider {providerId} not found.");
            }

            existing.Name = dto.ProviderName;
            existing.Npi = dto.ProviderNpi;
            existing.Taxonomy = dto.ProviderTaxonomy;
            existing.TelehealthEnrolled = dto.ProviderEnrolled;
            existing.ContactInfo = dto.ProviderContact;
            existing.Status = dto.ProviderStatus;

            await _repo.UpdateProviderAsync(existing);
        }

        public async Task DeleteProviderByIdAsync(int providerId)
        {
            var existing = await _repo.GetProviderByIdAsync(providerId);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Provider {providerId} not found.");
            }

            await _repo.DeleteProviderAsync(existing);
        }
    }
}