using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Models;
using Microsoft.EntityFrameworkCore;
using Telebill.Data;

namespace Telebill.Repositories.MasterData
{
    public class ProviderRepository: IProviderRepository
    {
        private readonly TeleBillContext _context;

        public ProviderRepository(TeleBillContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Provider>> GetAllProvidersAsync()
        {
            return await _context.Providers.ToListAsync();
        }

        public Task<Provider?> GetProviderByIdAsync(int providerId)
        {
            return _context.Providers.FirstOrDefaultAsync(p => p.ProviderId == providerId);
        }

        public Task<Provider?> GetProviderByNameAsync(string providerName)
        {
            return _context.Providers.FirstOrDefaultAsync(p => p.Name == providerName);
        }

        public Task<Provider?> GetProviderByNPIAsync(string npiId)
        {
            return _context.Providers.FirstOrDefaultAsync(p => p.Npi == npiId);
        }

        public Task<List<Provider>> GetActiveProvidersAsync()
        {
            return _context.Providers
                .Where(p => p.Status == "Active" && p.TelehealthEnrolled == true)
                .ToListAsync();
        }

        public async Task AddProviderAsync(Provider provider)
        {
            _context.Providers.Add(provider);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProviderAsync(Provider provider)
        {
            _context.Providers.Update(provider);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProviderAsync(Provider provider)
        {
            _context.Providers.Remove(provider);
            await _context.SaveChangesAsync();
        }
    }
}